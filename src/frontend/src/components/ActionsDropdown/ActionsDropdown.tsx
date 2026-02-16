import { useState, useRef, useEffect } from 'react';
import './ActionsDropdown.scss';

type ActionVariant = 'default' | 'danger' | 'status';

interface ActionItem {
  label: string;
  onClick: () => void;
  variant?: ActionVariant;
  disabled?: boolean;
}

interface ActionsDropdownProps {
  actions: ActionItem[];
  ariaLabel?: string;
}

type DropdownPosition = 'left' | 'right' | 'center';

const ActionsDropdown = ({ actions, ariaLabel }: ActionsDropdownProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [position, setPosition] = useState<DropdownPosition>('left');
  const dropdownRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  // Calculate dropdown position based on available viewport space
  useEffect(() => {
    if (isOpen && triggerRef.current) {
      const triggerRect = triggerRef.current.getBoundingClientRect();
      const viewportWidth = window.innerWidth;
      const dropdownWidth = 160;
      const margin = 20;

      if (triggerRect.left - dropdownWidth - margin >= 0) {
        setPosition('left');
      } else if (triggerRect.right + dropdownWidth + margin <= viewportWidth) {
        setPosition('right');
      } else {
        setPosition('center');
      }
    }
  }, [isOpen]);

  const handleToggle = (event: React.MouseEvent) => {
    event.stopPropagation();
    setIsOpen((prev) => !prev);
  };

  const handleAction = (event: React.MouseEvent, action: ActionItem) => {
    event.stopPropagation();
    action.onClick();
    setIsOpen(false);
  };

  const getItemClassName = (variant: ActionVariant = 'default') => {
    const base = 'actions-dropdown__item';
    if (variant === 'danger') return `${base} ${base}--danger`;
    if (variant === 'status') return `${base} ${base}--status`;
    return base;
  };

  return (
    <div className="actions-dropdown" ref={dropdownRef}>
      <button
        ref={triggerRef}
        type="button"
        className="actions-dropdown__trigger"
        onClick={handleToggle}
        aria-label={ariaLabel ?? 'Actions menu'}
      >
        <span className="actions-dropdown__dots">&#x22EF;</span>
      </button>

      {isOpen && (
        <div className={`actions-dropdown__menu actions-dropdown__menu--${position}`}>
          {actions.map((action, index) => (
            <button
              key={index}
              type="button"
              className={getItemClassName(action.variant)}
              onClick={(e) => handleAction(e, action)}
              disabled={action.disabled}
            >
              {action.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default ActionsDropdown;
