export interface SiteSettings {
  accessTokenExpirationMinutes: number;
  refreshTokenExpirationDays: number;
  loginCodeExpirationMinutes: number;
  loginCodeMaxAttempts: number;
  sessionExpiryWarningMinutes: number;
  isPersisted: boolean;
}

export interface SiteSettingsRequest {
  accessTokenExpirationMinutes: number;
  refreshTokenExpirationDays: number;
  loginCodeExpirationMinutes: number;
  loginCodeMaxAttempts: number;
  sessionExpiryWarningMinutes: number;
}
