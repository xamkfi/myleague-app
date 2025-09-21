import React from 'react';
import { Link } from 'react-router-dom';
import './Button.scss';

type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'link';
type ButtonSize = 'sm' | 'md' | 'lg';
type ButtonRounded = 'default' | 'pill';

export interface ButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  isLoading?: boolean;
  type?: 'button' | 'submit' | 'reset';
  variant?: ButtonVariant;
  size?: ButtonSize;
  rounded?: ButtonRounded;
  fullWidth?: boolean;
  ariaLabel?: string;
  className?: string;
  iconLeft?: string;
  iconRight?: string;
  to?: string;
}

const Button: React.FC<ButtonProps> = ({
  children,
  onClick,
  disabled = false,
  isLoading = false,
  type = 'button',
  variant = 'primary',
  size = 'md',
  rounded = 'default',
  fullWidth = false,
  ariaLabel,
  className = '',
  iconLeft,
  iconRight,
  to,
}) => {
  const classes = [
    'btn',
    `btn--${variant}`,
    `btn--${size}`,
    rounded === 'pill' ? 'btn--pill' : '',
    fullWidth ? 'btn--block' : '',
    isLoading ? 'btn--loading' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  const content = (
    <>
      {iconLeft && (
        <img src={iconLeft} alt="" aria-hidden="true" className="btn__icon btn__icon--left" />
      )}
      <span className="btn__label">{children}</span>
      {iconRight && (
        <img src={iconRight} alt="" aria-hidden="true" className="btn__icon btn__icon--right" />
      )}
    </>
  );

  if (to) {
    return (
      <Link
        to={to}
        aria-label={ariaLabel}
        className={classes}
        onClick={onClick}
      >
        {content}
      </Link>
    );
  }

  return (
    <button
      type={type}
      className={classes}
      onClick={onClick}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      aria-label={ariaLabel}
    >
      {content}
    </button>
  );
};

export default Button;


