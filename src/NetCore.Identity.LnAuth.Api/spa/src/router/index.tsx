import { createBrowserRouter } from "react-router-dom";
import Root from "../layout/root";
import ErrorPage from "../pages/error-page";
import { Login } from "../pages/login-page";
import ProtectedRoute from "./protected-route";
import HomePage from "../pages/home-page";
import { Register } from "../pages/register-page";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Root />,
    errorElement: <ErrorPage />,
    children: [
      {
        index: true,
        element: <Login />,
      },
      {
        path: "register",
        element: <Register></Register>,
      },
      {
        path: "home",
        element: (
          <ProtectedRoute>
            <HomePage></HomePage>
          </ProtectedRoute>
        ),
      },
    ],
  },
]);
