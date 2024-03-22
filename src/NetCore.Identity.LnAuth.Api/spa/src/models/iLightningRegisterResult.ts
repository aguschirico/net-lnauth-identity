export interface iLightningRegisterResult {
  success: boolean;
  reason: string;
  accessToken: string | null;
  refreshToken: string | null;
}
