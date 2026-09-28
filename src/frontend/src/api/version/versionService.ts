import { API_URL } from '../../constants/config';

let cachedVersion: string | null = null;
let versionRequest: Promise<string> | null = null;

/** Already loaded backend version, or null before the first successful fetch. */
export function peekBackendVersion(): string | null {
  return cachedVersion;
}

async function loadBackendVersion(): Promise<string> {
  const res = await fetch(`${API_URL}/version`);
  if (!res.ok) {
    return 'unknown';
  }
  const data: { version: string } = await res.json();
  return data.version ?? 'unknown';
}

export async function fetchBackendVersion(): Promise<string> {
  if (cachedVersion) {
    return cachedVersion;
  }

  if (!versionRequest) {
    versionRequest = loadBackendVersion()
      .then((version) => {
        if (version === 'unknown') {
          versionRequest = null;
          return version;
        }

        cachedVersion = version;
        return version;
      })
      .catch(() => {
        versionRequest = null;
        return 'unknown';
      });
  }

  return versionRequest;
}
