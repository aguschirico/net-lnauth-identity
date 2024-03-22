export interface iLightningLoginResult {
  success: boolean;
  reason: string;
  accessToken: string | null;
  refreshToken: string | null;
}
