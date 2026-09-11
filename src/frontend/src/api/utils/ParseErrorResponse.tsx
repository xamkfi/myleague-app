/**
 * Parses a response body (object or string) into a string suitable for ErrorPopup:
 * either JSON { title, errors } or a plain message.
 */
function parseErrorBody(body: unknown, defaultMessage: string): string {
  try {
    if (typeof body === 'string') {
      return body;
    }

    const obj = body as Record<string, unknown> | null | undefined;

    if (obj && typeof obj === 'object') {
      const getString = (o: Record<string, unknown>, key: string): string | undefined => {
        const v = o[key];
        return typeof v === 'string' ? v : undefined;
      };

      // ASP.NET Core ProblemDetails/ValidationProblemDetails
      if ('errors' in obj && obj.errors && typeof obj.errors === 'object') {
        const rawErrors = obj.errors as unknown;
        let aggregated: unknown[] = [];

        if (Array.isArray(rawErrors)) {
          aggregated = rawErrors as unknown[];
        } else {
          aggregated = Object.values(rawErrors as Record<string, unknown[]>)
            .reduce<unknown[]>((acc, val) => acc.concat(val || []), []);
        }

        const payload = {
          title: getString(obj, 'title') || getString(obj, 'detail') || getString(obj, 'message') || defaultMessage,
          errors: aggregated
        };
        return JSON.stringify(payload);
      }

      // Our ApiResponse shape
      if ('success' in obj && obj.success === false) {
        const payload = {
          title: getString(obj, 'message') || defaultMessage,
          errors: (obj.errors as unknown) || []
        };
        return JSON.stringify(payload);
      }

      return JSON.stringify(obj);
    }
  } catch (readError) {
    console.error('Error parsing error response:', readError);
  }

  return defaultMessage;
}

/**
 * Parses error responses for use with ErrorPopup.
 * Accepts either a fetch Response (reads body once) or already-parsed body (object/string).
 * Returns a string: JSON { title, errors } or a plain message.
 */
export async function parseErrorResponse(
  responseOrBody: Response | unknown,
  defaultMessage: string
): Promise<string> {
  if (responseOrBody instanceof Response) {
    const body = await responseOrBody.json().catch(() => responseOrBody.text());
    return parseErrorBody(body, defaultMessage);
  }
  return parseErrorBody(responseOrBody, defaultMessage);
}

/**
 * Turns a thrown API error (plain text or JSON `{ title, errors }`) into a single
 * user-facing sentence. ErrorPopup can still parse JSON if the caller prefers that shape.
 */
export function unwrapApiErrorMessage(err: unknown, fallback: string): string {
  const raw = err instanceof Error ? err.message : String(err ?? '');
  const stripped = raw.replace(/^Error:\s*/, '').trim();

  if (!stripped) {
    return fallback;
  }

  if (stripped.includes('Failed to fetch') || stripped.includes('NetworkError')) {
    return fallback;
  }

  try {
    const parsed = JSON.parse(stripped) as {
      title?: unknown;
      message?: unknown;
      errors?: unknown;
    };

    if (parsed && typeof parsed === 'object') {
      if (Array.isArray(parsed.errors)) {
        const first = parsed.errors.find(
          (item): item is string => typeof item === 'string' && item.trim().length > 0,
        );
        if (first) {
          return first;
        }
      }

      if (typeof parsed.title === 'string' && parsed.title.trim()) {
        return parsed.title;
      }

      if (typeof parsed.message === 'string' && parsed.message.trim()) {
        return parsed.message;
      }
    }
  } catch {
    // Already a plain message from the API or a local validation throw.
  }

  return stripped;
}