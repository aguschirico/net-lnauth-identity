import axios from "axios";

export const httpClient = axios.create({
  headers: {
    "Content-type": "application/json",
  },
});
