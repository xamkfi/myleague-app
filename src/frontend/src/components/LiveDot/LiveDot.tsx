import './LiveDot.scss';

interface LiveDotProps {
  /** Optional positive integer; when provided and >1 the dot becomes a small numeric badge. */
  count?: number;
  /** Accessible label describing what the dot indicates (e.g. "3 matches in progress"). */
  ariaLabel?: string;
  /** Visual variant: dark surface (sidebar) or light surface (table rows). */
  tone?: 'light' | 'dark';
  /** Extra class name for layout positioning. */
  className?: string;
}

const LiveDot = ({ count, ariaLabel, tone = 'light', className }: LiveDotProps) => {
  const showNumber: boolean = typeof count === 'number' && count > 1;
  const classes: string = [
    'live-dot',
    `live-dot--${tone}`,
    showNumber ? 'live-dot--with-count' : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <span
      className={classes}
      role="status"
      aria-label={ariaLabel}
      title={ariaLabel}
    >
      {showNumber ? <span className="live-dot__count">{count}</span> : null}
    </span>
  );
};

export default LiveDot;
