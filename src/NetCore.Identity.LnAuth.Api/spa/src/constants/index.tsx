export const API_BASE_URL = location.origin.startsWith("http://localhost")
  ? `${import.meta.env.VITE_APP_BACKEND_BASE_URL}`
  : `${location.origin}`;
