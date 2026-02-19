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