export interface SiteSettings {
  accessTokenExpirationMinutes: number;
  refreshTokenExpirationDays: number;
  loginCodeExpirationMinutes: number;
  loginCodeMaxAttempts: number;
  sessionExpiryWarningMinutes: number;
  isPersisted: boolean;
  playerLicenceResetMonth: number;
  playerLicenceResetDay: number;
  lastPlayerLicenceResetYear: number | null;
}

export interface SiteSettingsRequest {
  accessTokenExpirationMinutes: number;
  refreshTokenExpirationDays: number;
  loginCodeExpirationMinutes: number;
  loginCodeMaxAttempts: number;
  sessionExpiryWarningMinutes: number;
  playerLicenceResetMonth: number;
  playerLicenceResetDay: number;
}

export interface PlayerLicenceResetResult {
  ran: boolean;
  cutoffYear: number;
  floorballDeactivated: number;
  footballDeactivated: number;
  hockeyDeactivated: number;
}
