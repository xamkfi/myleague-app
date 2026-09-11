import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import './BulkActionsBar.scss';

type ActionVariant = 'default' | 'danger' | 'status';

interface BulkAction {
  label: string;
  onClick: () => void;
  variant?: ActionVariant;
  disabled?: boolean;
}

interface BulkActionsBarProps {
  selectedCount: number;
  totalCount: number;
  onSelectAll: () => void;
  onClearSelection: () => void;
  actions: BulkAction[];
}

export default function BulkActionsBar({
  selectedCount,
  totalCount,
  onSelectAll,
  onClearSelection,
  actions,
}: BulkActionsBarProps) {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

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

  if (selectedCount === 0) {
    return null;
  }

  const getItemClassName = (variant: ActionVariant = 'default') => {
    const base = 'bulk-actions-bar__item';
    if (variant === 'danger') return `${base} ${base}--danger`;
    if (variant === 'status') return `${base} ${base}--status`;
    return base;
  };

  const handleAction = (action: BulkAction) => {
    action.onClick();
    setIsOpen(false);
  };

  return (
    <div className="bulk-actions-bar">
      <div className="bulk-actions-bar__left">
        <span className="bulk-actions-bar__count">
          {t('common.bulk.selected', '{{count}} valittu', { count: selectedCount })}
        </span>
        {selectedCount < totalCount && (
          <button
            type="button"
            className="bulk-actions-bar__link-btn"
            onClick={onSelectAll}
          >
            {t('common.bulk.selectAll', 'Valitse kaikki ({{count}})', { count: totalCount })}
          </button>
        )}
        <button
          type="button"
          className="bulk-actions-bar__link-btn"
          onClick={onClearSelection}
        >
          {t('common.bulk.clear', 'Tyhjennä')}
        </button>
      </div>

      <div className="bulk-actions-bar__right" ref={dropdownRef}>
        <button
          type="button"
          className="bulk-actions-bar__actions-btn"
          onClick={() => setIsOpen((prev) => !prev)}
        >
          {t('common.bulk.actions', 'Toiminnot')}
          <span className="bulk-actions-bar__chevron">{isOpen ? '▲' : '▼'}</span>
        </button>

        {isOpen && (
          <div className="bulk-actions-bar__menu">
            {actions.map((action, index) => (
              <button
                key={index}
                type="button"
                className={getItemClassName(action.variant)}
                onClick={() => handleAction(action)}
                disabled={action.disabled}
              >
                {action.label}
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
