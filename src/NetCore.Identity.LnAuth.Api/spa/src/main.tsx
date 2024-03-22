import React from "react";
import ReactDOM from "react-dom/client";

import { RouterProvider } from "react-router-dom";
import { router } from "./router/index.tsx";
import { store } from "./redux/store.ts";
import AxiosApiInterceptor from "./services/axios-api-interceptor.tsx";
import { Provider } from "react-redux";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <Provider store={store}>
      <AxiosApiInterceptor />
      <RouterProvider router={router} />
    </Provider>
  </React.StrictMode>
);
