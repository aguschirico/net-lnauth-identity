import { useEffect } from "react";
import { refreshToken } from "../services/api.auth";
import { httpClient } from "../services/http-client";
import { useAppDispatch, useAppSelector } from "../redux/hooks";
import {
  resetToken,
  selectAuth,
  updateToken,
} from "../redux/features/auth.slice";

const AxiosApiInterceptor = () => {
  const authData = useAppSelector(selectAuth);
  const dispatch = useAppDispatch();
  useEffect(() => {
    const requestInterceptor = httpClient.interceptors.request.use(
      async (config) => {
        const accessToken = authData.accessToken;
        if (accessToken && !config.headers.Authorization) {
          config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
      }
    );

    // Response interceptor
    const responseInterceptor = httpClient.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response && error.response.status === 401) {
          // Token expired, attempt to refresh it
          if (authData.refreshToken && authData.accessToken) {
            // Make a refresh token request and update access token
            try {
              const response = await refreshToken({
                accessToken: authData.accessToken,
                refreshToken: authData.refreshToken,
              });
              if (response && response.isSucceed && response.data) {
                dispatch(updateToken(response.data));
                error.config.headers.Authorization = `Bearer ${response.data.accessToken}`;
                return httpClient.request(error.config);
              } else {
                dispatch(resetToken());
              }
            } catch (refreshError) {
              dispatch(resetToken());
              throw refreshError;
            }
          } else {
            dispatch(resetToken());
          }
        }

        return Promise.reject(error);
      }
    );

    return () => {
      // Cleanup: Remove the interceptors when the component unmounts
      httpClient.interceptors.request.eject(requestInterceptor);
      httpClient.interceptors.response.eject(responseInterceptor);
    };
  }, [authData, dispatch]);

  return null; // This component doesn't render anything
};

export default AxiosApiInterceptor;
