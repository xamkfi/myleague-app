interface FieldErrorProps {
  /** Referenced by the field's `aria-describedby`. */
  id: string;
  message?: string | null;
}

/** Validation message under a news editor field. Renders nothing without a message. */
export default function FieldError({ id, message }: FieldErrorProps) {
  if (!message) {
    return null;
  }
  return (
    <p id={id} className="news-field__error">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
        <circle cx="12" cy="12" r="9" />
        <path d="M12 8v4m0 4h.01" />
      </svg>
      {message}
    </p>
  );
}
