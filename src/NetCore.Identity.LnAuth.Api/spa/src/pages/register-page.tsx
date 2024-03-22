import { App, Button, Form, FormInstance, Input, Result } from "antd";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAppDispatch } from "../redux/hooks";
import { resetLoading, setLoading } from "../redux/features/auth.slice";
import { register } from "../services/api.auth";
import LightningRegister from "../components/register-lightning";

type FieldType = {
  email?: string;
  password?: string;
};

export const Register = () => {
  const formRef = React.useRef<FormInstance>(null);
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { message } = App.useApp();
  const [registerWithLightning, setRegisterWithLightning] = useState(false);
  const [registered, setRegistered] = useState(false);

  const onFinish = async (values: FieldType) => {
    dispatch(setLoading());

    const data = await register(
      values.email as string,
      values.password as string
    );
    dispatch(resetLoading());
    if (data?.isSucceed) {
      setRegistered(true);
    } else if (data?.messages) {
      data.messages?.DuplicateUserName &&
        formRef.current?.setFields([
          { name: "email", errors: data.messages?.DuplicateUserName },
        ]);
      data.messages?.password &&
        formRef.current?.setFields([
          { name: "password", errors: data.messages?.password },
        ]);
    } else {
      message.error("Unexpected error occurred please try again later.");
    }
  };
  return (
    <>
      {registered && (
        <Result
          status="success"
          title="Registro exitoso!"
          subTitle="Haga click en login para entrar"
          extra={[
            <button key="signin" onClick={() => navigate("/login")}>
              Log in
            </button>,
          ]}
        />
      )}
      {!registered && (
        <div>
          <div>
            <Button onClick={() => setRegisterWithLightning(true)}>
              Registro con Lightning Auth
            </Button>
          </div>
          {registerWithLightning ? (
            <LightningRegister
              setRegisterWithLightning={setRegisterWithLightning}
              messageApi={message}
            ></LightningRegister>
          ) : (
            <Form
              name="basic"
              labelCol={{ span: 8 }}
              wrapperCol={{ span: 16 }}
              style={{ maxWidth: 600 }}
              initialValues={{ remember: true }}
              onFinish={onFinish}
              autoComplete="off"
              ref={formRef}
            >
              <Form.Item<FieldType>
                label="Email"
                name="email"
                rules={[
                  {
                    required: true,
                    message: "Please input your Email!",
                  },
                  {
                    type: "email",
                    message: "Please enter valid Email!",
                  },
                ]}
              >
                <Input />
              </Form.Item>

              <Form.Item<FieldType>
                label="Password"
                name="password"
                extra={
                  <ul>
                    <li>
                      Minimum Length: The password must be at least 6 characters
                      long.
                    </li>
                    <li>
                      At least 1 Uppercase Letter: Include at least one
                      uppercase letter (A-Z).
                    </li>
                    <li>
                      At least 1 Lowercase Letter: Include at least one
                      lowercase letter (a-z).
                    </li>
                    <li>
                      At least 1 Special Character: Include at least one special
                      character (e.g., !@#$%^&*).
                    </li>
                  </ul>
                }
                rules={[
                  { required: true, message: "Please input your password!" },
                  {
                    pattern: new RegExp(
                      "^(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*]).{6,}$"
                    ),
                    message: "Please check password requirements",
                  },
                ]}
              >
                <Input.Password />
              </Form.Item>

              <Form.Item wrapperCol={{ offset: 8, span: 16 }}>
                <Button type="primary" htmlType="submit">
                  Submit
                </Button>
              </Form.Item>
            </Form>
          )}
        </div>
      )}
    </>
  );
};
