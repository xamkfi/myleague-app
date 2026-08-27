import DOMPurify from 'dompurify';

/**
 * Returns a sanitized, inert HTML tree. Never written into the live document.
 * Match-result JSON payloads live in application/json script tags; those are
 * kept, other scripts are stripped.
 */
export function parseSanitizedHtmlRoot(html: string): Element {
  const sanitized: Node = DOMPurify.sanitize(html, {
    RETURN_DOM: true,
    ADD_TAGS: ['script'],
    ADD_ATTR: ['type'],
  });
  const root = sanitized instanceof Element ? sanitized : document.createElement('div');

  root.querySelectorAll('script').forEach((script) => {
    const isMatchPayload =
      script.getAttribute('type') === 'application/json' &&
      script.classList.contains('match-result-data');
    if (!isMatchPayload) {
      script.remove();
    }
  });

  return root;
}
