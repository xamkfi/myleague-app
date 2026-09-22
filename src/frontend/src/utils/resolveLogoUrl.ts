export function resolveLogoUrl(url: string | null | undefined): string | undefined {
  const trimmed = url?.trim();
  if (!trimmed) {
    return undefined;
  }

  try {
    const hostname = new URL(trimmed, 'https://myleague.invalid').hostname.toLowerCase();
    if (hostname === 'example.com' || hostname.endsWith('.example.com')) {
      return undefined;
    }
  } catch {
    return undefined;
  }

  return trimmed;
}
