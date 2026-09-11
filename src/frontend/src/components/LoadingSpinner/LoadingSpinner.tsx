import './LoadingSpinner.scss';

interface LoadingSpinnerProps {
  /** Spinner size: sm (24px), md (40px, default), lg (48px) */
  size?: 'sm' | 'md' | 'lg';
  /** Optional text displayed below the spinner */
  text?: string;
  /** Use 'light' on dark backgrounds, 'dark' (default) on light backgrounds */
  variant?: 'light' | 'dark';
}

export default function LoadingSpinner({
  size = 'md',
  text,
  variant = 'dark',
}: LoadingSpinnerProps) {
  const classes = [
    'loading-spinner',
    size !== 'md' ? `loading-spinner--${size}` : '',
    variant === 'light' ? 'loading-spinner--light' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={classes}>
      <div className="loading-spinner__circle" />
      {text && <span className="loading-spinner__text">{text}</span>}
    </div>
  );
}
