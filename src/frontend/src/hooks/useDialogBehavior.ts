import { useEffect, useRef, type RefObject } from 'react';

const FOCUSABLE_SELECTOR = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

/**
 * Modal dialog basics: Escape closes, Tab stays inside the dialog, the page behind
 * does not scroll, and focus returns to the element that opened the dialog.
 *
 * @param open Whether the dialog is shown.
 * @param dialogRef The element with `role="dialog"`. It should have `tabIndex={-1}`.
 * @param onClose Called on Escape.
 * @param initialFocusRef Element to focus on open. Falls back to the dialog itself.
 */
export function useDialogBehavior(
  open: boolean,
  dialogRef: RefObject<HTMLElement | null>,
  onClose: () => void,
  initialFocusRef?: RefObject<HTMLElement | null>,
): void {
  const onCloseRef = useRef(onClose);

  useEffect(() => {
    onCloseRef.current = onClose;
  }, [onClose]);

  useEffect(() => {
    if (!open) {
      return undefined;
    }

    const previouslyFocused: HTMLElement | null =
      document.activeElement instanceof HTMLElement ? document.activeElement : null;
    const previousOverflow: string = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    (initialFocusRef?.current ?? dialogRef.current)?.focus();

    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === 'Escape') {
        event.stopPropagation();
        onCloseRef.current();
        return;
      }

      const dialog: HTMLElement | null = dialogRef.current;
      if (event.key !== 'Tab' || !dialog) {
        return;
      }

      const focusable: HTMLElement[] = Array.from(
        dialog.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR),
      ).filter((element) => element.offsetParent !== null);
      if (focusable.length === 0) {
        event.preventDefault();
        return;
      }

      const first: HTMLElement = focusable[0];
      const last: HTMLElement = focusable[focusable.length - 1];
      const active: Element | null = document.activeElement;
      if (event.shiftKey && (active === first || active === dialog)) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = previousOverflow;
      previouslyFocused?.focus();
    };
  }, [open, dialogRef, initialFocusRef]);
}
