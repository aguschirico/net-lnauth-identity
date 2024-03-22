import { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAppSelector } from "../redux/hooks";
import { selectAuth } from "../redux/features/auth.slice";

type ProtectedRouteProps = {
  children: ReactNode;
};

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const auth = useAppSelector(selectAuth);

  return auth.isAuthenticated ? children : <Navigate to="/" replace />;
};

export default ProtectedRoute;
