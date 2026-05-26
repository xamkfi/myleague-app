import { useState, useRef, useEffect } from 'react';
import {createPortal} from 'react-dom';
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


  
const ActionsDropdown = ({ actions, ariaLabel }: ActionsDropdownProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  // Stores dropdown position styles dynamically
  const [menuStyle, setMenuStyle] = useState<React.CSSProperties>({});
  const menuRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node;

      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(target) &&
        menuRef.current &&
        !menuRef.current.contains(target)
      ) {
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

// Calculate menu position based on trigger button location  
  useEffect(() => {
  if (isOpen && triggerRef.current) {
    const rect = triggerRef.current.getBoundingClientRect();

    setMenuStyle({
      position: 'fixed',
      top: rect.bottom + 6,
      right: window.innerWidth - rect.right,
      zIndex: 99999,
    });
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

      {/* Render menu outside parent container to avoid clipping */}
      {isOpen &&
        createPortal(
          <div
            ref={menuRef}
            className="actions-dropdown__menu"
            style={menuStyle}
            onClick={(e) => e.stopPropagation()}
            onMouseDown={(e) => e.stopPropagation()}
          >
            {actions.map((action, index) => (
              <button
                key={index}
                type="button"
                className={getItemClassName(action.variant)}
                onMouseDown={(e) => e.stopPropagation()}
                onClick={(e) => handleAction(e, action)}
                disabled={action.disabled}
              >
                {action.label}
              </button>
            ))}
          </div>,
          document.body
        )}
    </div>
  );
};

export default ActionsDropdown;
