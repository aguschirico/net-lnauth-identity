import { httpClient } from "./http-client";
import { API_BASE_URL } from "../constants";
import { iAppResponse } from "../models/iAppResponse";
const BASE_URL = `${API_BASE_URL}/api/auth`;

export const register = async (email: string, password: string) => {
  const response = await httpClient
    // eslint-disable-next-line @typescript-eslint/ban-types
    .post<iAppResponse<{}>>(`${BASE_URL}/register`, {
      email: email,
      password: password,
    })
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};

export const login = async (email: string, password: string) => {
  const response = await httpClient
    .post<iAppResponse<{ accessToken: string; refreshToken: string }>>(
      `${BASE_URL}/login`,
      {
        email: email,
        password: password,
      }
    )
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};

export const lnEncodedLoginUrl = async (connectionId: string) => {
  const response = await httpClient
    .get<iAppResponse<{ encodedUrl: string }>>(
      `${BASE_URL}/encoded-login-url?connectionId=${connectionId}`
    )
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};

export const lnEncodedRegisterUrl = async (connectionId: string) => {
  const response = await httpClient
    .get<iAppResponse<{ encodedUrl: string }>>(
      `${BASE_URL}/encoded-register-url?connectionId=${connectionId}`
    )
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};

export const refreshToken = async (data: {
  accessToken: string;
  refreshToken: string;
}) => {
  const response = await httpClient
    .post<iAppResponse<{ accessToken: string; refreshToken: string }>>(
      `${BASE_URL}/refresh-token`,
      data
    )
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};

export const logout = async () => {
  const response = await httpClient
    .post<iAppResponse<boolean>>(`${BASE_URL}/logout`)
    .catch((ex: Error) => {
      console.log(ex);
    });
  return response?.data;
};
