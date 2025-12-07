/**
 * Helper function to parse error responses properly
 */
export async function parseErrorResponse(
  response: unknown,
  defaultMessage: string
): Promise<string> {
  try {
    // If we already have a string, just return it
    if (typeof response === 'string') {
      return response;
    }

    const obj = response as Record<string, unknown> | null | undefined;

    if (obj && typeof obj === 'object') {
      const getString = (o: Record<string, unknown>, key: string): string | undefined => {
        const v = o[key];
        return typeof v === 'string' ? v : undefined;
      };

      // ASP.NET Core ProblemDetails/ValidationProblemDetails
      if ('errors' in obj && obj.errors && typeof obj.errors === 'object') {
        // errors can be: Record<string, string[]> or string[]
        const rawErrors = obj.errors as unknown;
        let aggregated: unknown[] = [];

        if (Array.isArray(rawErrors)) {
          aggregated = rawErrors as unknown[];
        } else {
          // Flatten Record<string, string[]>
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

      // Fallback to raw object
      return JSON.stringify(obj);
    }
  } catch (readError) {
    console.error('Error parsing error response:', readError);
  }

  return `${defaultMessage}`;
}