import { App, Button, Form, FormInstance, Input } from "antd";
import React, { useState } from "react";
import { login } from "../services/api.auth";
import { NavLink, useNavigate } from "react-router-dom";
import { useAppDispatch } from "../redux/hooks";
import {
  resetLoading,
  setLoading,
  updateToken,
} from "../redux/features/auth.slice";
import LightningLogin from "../components/login-lightning";
type FieldType = {
  email?: string;
  password?: string;
};

export const Login = () => {
  const navigate = useNavigate();
  const [loginWithLightning, setLoginWithLightning] = useState(false);

  const dispatch = useAppDispatch();
  const { message } = App.useApp();
  const formRef = React.useRef<FormInstance>(null);
  const onFinish = async (values: FieldType) => {
    dispatch(setLoading());
    const data = await login(values.email as string, values.password as string);
    dispatch(resetLoading());
    if (data?.isSucceed && data?.data) {
      message.success("Login exitoso.");
      dispatch(updateToken(data.data));
      navigate("/home");
    } else if (data != null) {
      data?.messages?.email &&
        formRef.current?.setFields([
          { name: "email", errors: data.messages?.email },
        ]);
      data?.messages?.password &&
        formRef.current?.setFields([
          { name: "password", errors: data.messages?.password },
        ]);
    } else {
      message.error("Error desconocido.");
    }
  };
  return (
    <>
      <Button type="primary" onClick={() => setLoginWithLightning(true)}>
        Login con lightning
      </Button>
      {loginWithLightning ? (
        <LightningLogin
          setLoginWithLightning={setLoginWithLightning}
          messageApi={message}
        ></LightningLogin>
      ) : (
        <Form
          name="unified-app-login"
          id="unified-app-login"
          labelCol={{ span: 8 }}
          wrapperCol={{ span: 16 }}
          style={{ maxWidth: 600 }}
          initialValues={{ remember: true }}
          onFinish={onFinish}
          autoComplete="login"
          ref={formRef}
        >
          <Form.Item<FieldType>
            label="email"
            name="email"
            rules={[{ required: true, message: "Please input your Email!" }]}
          >
            <Input
              name="login-name"
              id="login-name"
              autoComplete="login-name"
            />
          </Form.Item>

          <Form.Item<FieldType>
            label="Password"
            name="password"
            rules={[{ required: true, message: "Introducir una constraseña!" }]}
          >
            <Input.Password
              name="login-password"
              id="login-password"
              autoComplete="login-password"
            />
          </Form.Item>

          <Form.Item wrapperCol={{ offset: 8, span: 16 }}>
            <Button type="primary" htmlType="submit">
              Submit
            </Button>
          </Form.Item>
        </Form>
      )}

      <NavLink to="/register">Crear usuario</NavLink>
    </>
  );
};
