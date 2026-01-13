import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { DivisionType } from '../../../../types/common/divisionType';

interface DivisionActionsDropdownProps {
  division: DivisionType;
  onEdit: (divisionId: string) => void;
  onToggleStatus: (division: DivisionType) => void;
  onDelete: (division: DivisionType) => void;
  statusUpdatingId?: string | null;
}

const DivisionActionsDropdown = ({
  division,
  onEdit,
  onToggleStatus,
  onDelete,
  statusUpdatingId,
}: DivisionActionsDropdownProps) => {
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

  const handleEdit = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    onEdit(division.id);
    setIsOpen(false);
  };

  const handleToggleStatus = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    onToggleStatus(division);
    setIsOpen(false);
  };

  const handleDelete = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    onDelete(division);
    setIsOpen(false);
  };

  return (
    <div className="division-actions-dropdown" ref={dropdownRef}>
      <button
        type="button"
        className="dropdown-trigger"
        onClick={(event) => {
          event.stopPropagation();
          setIsOpen((prev) => !prev);
        }}
        aria-label={t('admin.divisions.actions.menu', 'Division actions menu')}
      >
        <span className="three-dots">⋯</span>
      </button>

      {isOpen && (
        <div className="dropdown-menu">
          <button type="button" className="dropdown-item" onClick={handleEdit}>
            {t('common.edit', 'Edit')}
          </button>
          <button
            type="button"
            className="dropdown-item status-item"
            onClick={handleToggleStatus}
            disabled={statusUpdatingId === division.id}
          >
            {division.isActive
              ? t('admin.divisions.actions.deactivate', 'Deactivate')
              : t('admin.divisions.actions.activate', 'Activate')}
          </button>
          <button type="button" className="dropdown-item delete-item" onClick={handleDelete}>
            {t('common.delete', 'Delete')}
          </button>
        </div>
      )}
    </div>
  );
};

export default DivisionActionsDropdown;

