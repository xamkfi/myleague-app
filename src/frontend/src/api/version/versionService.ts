import { API_URL } from '../../constants/config';

export async function fetchBackendVersion(): Promise<string> {
  try {
    const res = await fetch(`${API_URL}/version`);
    if (!res.ok) return 'unknown';
    const data: { version: string } = await res.json();
    return data.version ?? 'unknown';
  } catch {
    return 'unknown';
  }
}
