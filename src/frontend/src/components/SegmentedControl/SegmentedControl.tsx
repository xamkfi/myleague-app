import type { ReactNode } from 'react';
import './SegmentedControl.scss';

export interface SegmentedOption<T extends string> {
  value: T;
  label: string;
  /** Rendered before the label. Receives whether this option is the active one. */
  icon?: (active: boolean) => ReactNode;
}

interface SegmentedControlProps<T extends string> {
  options: SegmentedOption<T>[];
  value: T | null;
  onChange: (value: T) => void;
  ariaLabel: string;
  /** `sport` colours the active option with the accent of the selected sport. */
  tone?: 'neutral' | 'sport';
  size?: 'sm' | 'md';
  /** `inline` is one pill row; `stretch` fills the width; `grid` wraps two per row. */
  layout?: 'inline' | 'stretch' | 'grid';
  className?: string;
}

/** A row of mutually exclusive toggle buttons, styled like the admin sport tabs. */
export default function SegmentedControl<T extends string>({
  options,
  value,
  onChange,
  ariaLabel,
  tone = 'neutral',
  size = 'md',
  layout = 'inline',
  className,
}: SegmentedControlProps<T>) {
  const classes: string = [
    'segmented-control',
    `segmented-control--${size}`,
    `segmented-control--${layout}`,
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div
      className={classes}
      role="group"
      aria-label={ariaLabel}
      data-accent={tone === 'sport' && value ? value : undefined}
    >
      {options.map((option) => {
        const active: boolean = option.value === value;
        return (
          <button
            key={option.value}
            type="button"
            className={`segmented-control__option${active ? ' segmented-control__option--active' : ''}`}
            aria-pressed={active}
            onClick={() => onChange(option.value)}
          >
            {option.icon?.(active)}
            <span className="segmented-control__label">{option.label}</span>
          </button>
        );
      })}
    </div>
  );
}
