import { PayloadAction, createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { jwtDecode } from "jwt-decode";
import { logout } from "../../services/api.auth";
import { RootState } from "../store";

export interface iUser {
  Id: string;
  RoleClaim: Array<string>;
  UserName: string;
}

export interface iAuthState {
  status: "idle" | "loading" | "failed";
  accessToken?: string;
  refreshToken?: string;
  user?: iUser;
  isAuthenticated: boolean;
}

const initialState: iAuthState = {
  status: "idle",
  isAuthenticated: false,
};

export const logoutAsync = createAsyncThunk("api/auth/logout", async () => {
  const response = await logout();
  // The value we return becomes the `fulfilled` action payload
  return response?.data;
});

export const authSlice = createSlice({
  name: "auth",
  initialState,
  // The `reducers` field lets us define reducers and generate associated actions
  reducers: {
    updateToken: (
      state,
      action: PayloadAction<{ accessToken: string; refreshToken: string }>
    ) => {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.user = jwtDecode<iUser>(action.payload.accessToken);
      state.isAuthenticated = true;
    },
    resetToken: (state) => {
      state.accessToken = undefined;
      state.refreshToken = undefined;
      state.user = undefined;
      state.isAuthenticated = false;
    },
    setLoading: (state) => {
      state.status = "loading";
    },
    resetLoading: (state) => {
      state.status = "idle";
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(logoutAsync.pending, (state) => {
        state.status = "loading";
      })
      .addCase(logoutAsync.fulfilled, (state) => {
        state.status = "idle";
        state.accessToken = undefined;
        state.refreshToken = undefined;
        state.user = undefined;
        state.isAuthenticated = false;
      })
      .addCase(logoutAsync.rejected, (state) => {
        state.status = "failed";
        state.isAuthenticated = false;
      });
  },
});

export const { updateToken, resetToken, setLoading, resetLoading } =
  authSlice.actions;
export const selectAuth = (state: RootState) => state.auth;
export default authSlice.reducer;
