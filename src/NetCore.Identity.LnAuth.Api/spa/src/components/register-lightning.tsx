import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAppDispatch } from "../redux/hooks";
import { resetLoading, setLoading } from "../redux/features/auth.slice";
import { QRCode, Result } from "antd";
import { lnEncodedRegisterUrl } from "../services/api.auth";
import { iLightningRegisterResult } from "../models/iLightningRegisterResult";
import { MessageInstance } from "antd/es/message/interface";
import { HubConnection } from "@microsoft/signalr";
import { API_BASE_URL } from "../constants";
import useSignalRHub from "../hooks/signalr";

type LightningRegisterProps = {
  setRegisterWithLightning: (val: boolean) => void;
  messageApi: MessageInstance;
};

const LightningRegister = ({
  setRegisterWithLightning,
  messageApi,
}: LightningRegisterProps) => {
  const [qrCode, setQrCode] = useState("");
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [registered, setRegistered] = useState(false);

  const handleRegisterCallback = (hub: HubConnection) => {
    hub.on(
      "ReceiveLightningRegisterResult",
      (lightningRegisterResult: iLightningRegisterResult) => {
        if (lightningRegisterResult.success) {
          setRegistered(true);
        } else {
          messageApi.error(lightningRegisterResult.reason);
        }
      }
    );
  };

  useSignalRHub(`${API_BASE_URL}/hubs/lightning-auth`, {
    onConnected: async (hub) => {
      dispatch(setLoading());
      await getEncodedUrlForRegister(hub.connectionId!);
      handleRegisterCallback(hub);
      dispatch(resetLoading());
    },
    onReconnecting: async () => {
      dispatch(setLoading());
    },
    onReconnected: async (connectionId) => {
      await getEncodedUrlForRegister(connectionId!);
      dispatch(resetLoading());
    },
    onError: async (error: Error | undefined) => {
      messageApi.error(error?.message);
      dispatch(resetLoading());
    },
    automaticReconnect: true,
  });

  const getEncodedUrlForRegister = async (connectionId: string) => {
    dispatch(setLoading());
    const data = await lnEncodedRegisterUrl(connectionId);

    if (data?.isSucceed && data?.data) {
      setQrCode(data?.data?.encodedUrl ?? "");
    } else {
      messageApi.error("Error en el registro");
    }
    dispatch(resetLoading());
  };

  return (
    <>
      {registered && (
        <Result
          status="success"
          title="Registro completado!"
          subTitle="Haga click en el botón de login"
          extra={[
            <button
              className="btn-normal"
              key="signin"
              onClick={() => navigate("/login")}
            >
              Log in
            </button>,
          ]}
        />
      )}
      {qrCode && !registered && (
        <>
          <div>
            <span>
              Volver al registro normal
              <a href="#" onClick={() => setRegisterWithLightning(false)}>
                haciendo click aqui
              </a>
            </span>
          </div>
          <div>
            <div>
              <span>Scan</span>
            </div>
            <QRCode
              errorLevel="H"
              size={250}
              iconSize={250 / 4}
              value={qrCode}
            />
          </div>
        </>
      )}
    </>
  );
};

export default LightningRegister;
