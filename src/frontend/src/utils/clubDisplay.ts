import { resolveLogoUrl } from './resolveLogoUrl';

const PLACEHOLDER_EMAIL = 'contact@example.com';

export function clubText(value: string | null | undefined): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}

export function clubEmail(value: string | null | undefined): string | null {
  const trimmed = clubText(value);
  if (!trimmed || trimmed.toLowerCase() === PLACEHOLDER_EMAIL) {
    return null;
  }
  return trimmed;
}

export function clubFoundingYear(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }
  const year = date.getUTCFullYear();
  if (year <= 1) {
    return null;
  }
  return String(year);
}

export function clubPublicUrl(value: string | null | undefined): string | null {
  return resolveLogoUrl(value) ?? null;
}
