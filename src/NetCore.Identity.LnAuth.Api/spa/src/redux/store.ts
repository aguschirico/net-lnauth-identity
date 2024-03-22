import {
  configureStore,
  ThunkAction,
  Action,
  combineReducers,
  Tuple,
} from "@reduxjs/toolkit";
import storage from "redux-persist/lib/storage";
import { persistReducer, persistStore } from "redux-persist";
import { thunk } from "redux-thunk";
import authReducer from "./features/auth.slice";

const authPersistConfig = {
  key: "auth",
  storage: storage,
};

const rootReducer = combineReducers({
  auth: persistReducer(authPersistConfig, authReducer),
});

export const store = configureStore({
  reducer: rootReducer,
  devTools: !import.meta.env.PROD,
  middleware: () => new Tuple(thunk),
});

export const persister = persistStore(store);

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
