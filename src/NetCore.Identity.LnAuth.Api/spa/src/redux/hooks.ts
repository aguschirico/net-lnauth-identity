import { TypedUseSelectorHook, useDispatch, useSelector } from "react-redux";
import type { RootState } from "../redux/store";
import { AnyAction, ThunkDispatch } from "@reduxjs/toolkit";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type AppThunkDispatch = ThunkDispatch<RootState, any, AnyAction>;
// Use throughout the app instead of plain `useDispatch` and `useSelector`
export const useAppDispatch: () => AppThunkDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
