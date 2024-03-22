import { App } from "antd";
import { Outlet } from "react-router-dom";
const Root = () => {
  return (
    <App>
      <Outlet />
    </App>
  );
};

export default Root;
