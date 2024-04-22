import { Button, QRCode } from "antd";
import { lnEncodedLoginUrl } from "../services/api.auth";
import { resetLoading, updateToken } from "../redux/features/auth.slice";
import { iLightningLoginResult } from "../models/iLightningLoginResult";
import { HubConnection } from "@microsoft/signalr";
import { API_BASE_URL } from "../constants";
import useSignalRHub from "../hooks/signalr";
import { useAppDispatch } from "../redux/hooks";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { MessageInstance } from "antd/es/message/interface";

type LightningLoginProps = {
  setLoginWithLightning: (val: boolean) => void;
  messageApi: MessageInstance;
};

const LightningLogin = ({
  setLoginWithLightning,
  messageApi,
}: LightningLoginProps) => {
  const [qrCode, setQrCode] = useState("");
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  useSignalRHub(`${API_BASE_URL}/hubs/lightning-auth`, {
    onConnected: async (hub: HubConnection) => {
      await getEncodedUrlForLogin(hub!.connectionId!);
      handleLoginCallback(hub);
    },
    onReconnected: async (connectionId: string | undefined) => {
      await getEncodedUrlForLogin(connectionId!);
    },
  });

  const handleLoginCallback = (hub: HubConnection) => {
    hub.on(
      "ReceiveLightningLoginResult",
      (authResult: iLightningLoginResult) => {
        if (authResult.success) {
          dispatch(
            updateToken({
              accessToken: authResult.accessToken!,
              refreshToken: authResult.refreshToken!,
            })
          );
          messageApi.success("Login exitoso!", 3).then(() => {
            navigate("/dashboard");
          });
        } else {
          messageApi.error(authResult.reason);
        }
      }
    );
  };
  const getEncodedUrlForLogin = async (connectionId: string) => {
    const data = await lnEncodedLoginUrl(connectionId);
    if (data?.isSucceed && data.data) {
      setQrCode(data?.data?.encodedUrl ?? "");
    } else {
      messageApi.error("Error al intentar loguearse.");
    }
    dispatch(resetLoading());
  };

  return (
    <>
      {qrCode && (
        <>
          <div>
            <Button type="primary" onClick={() => setLoginWithLightning(false)}>
              Volver a login normal
            </Button>
          </div>
          <div>
            <div>
              <span>Escanear </span>
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

export default LightningLogin;
