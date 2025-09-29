import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';

interface PlayerActionsDropdownProps {
  player: FloorballPlayerDto;
  onDelete: (playerId: string) => void;
  onStatusChange: (playerId: string, isActive: boolean) => void;
}

const PlayerActionsDropdown = ({ player, onDelete, onStatusChange }: PlayerActionsDropdownProps) => {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [dropdownPosition, setDropdownPosition] = useState<'left' | 'right' | 'center'>('left');
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

  // Calculate dropdown position based on available space
  useEffect(() => {
    if (isOpen && triggerRef.current) {
      const triggerRect = triggerRef.current.getBoundingClientRect();
      const viewportWidth = window.innerWidth;
      const dropdownWidth = 150; // min-width from CSS
      const margin = 20; // Safety margin from viewport edge
      
      // Since this is typically in the rightmost column, prefer left positioning
      // Check if there's enough space on the left first
      if (triggerRect.left - dropdownWidth - margin >= 0) {
        setDropdownPosition('left');
      } else if (triggerRect.right + dropdownWidth + margin <= viewportWidth) {
        // If not enough space on left, try right
        setDropdownPosition('right');
      } else {
        // If neither side works, center it
        setDropdownPosition('center');
      }
    }
  }, [isOpen]);

  const handleToggleDropdown = (e: React.MouseEvent) => {
    e.stopPropagation();
    setIsOpen(!isOpen);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete(player.id);
    setIsOpen(false);
  };

  const handleStatusChange = (e: React.MouseEvent) => {
    e.stopPropagation();
    onStatusChange(player.id, !player.isActive);
    setIsOpen(false);
  };

  return (
    <div className="player-actions-dropdown" ref={dropdownRef}>
      <button
        ref={triggerRef}
        className="dropdown-trigger"
        onClick={handleToggleDropdown}
        aria-label={t('floorball.players.actions.menu', 'Player actions menu')}
      >
        <span className="three-dots">⋯</span>
      </button>
      
      {isOpen && (
        <div className={`dropdown-menu dropdown-position-${dropdownPosition}`}>
          <button
            className="dropdown-item status-item"
            onClick={handleStatusChange}
          >
            {player.isActive 
              ? t('floorball.players.actions.deactivate', 'Deactivate Player')
              : t('floorball.players.actions.activate', 'Activate Player')
            }
          </button>
          <button
            className="dropdown-item delete-item"
            onClick={handleDelete}
          >
            {t('common.delete', 'Delete')}
          </button>
        </div>
      )}
    </div>
  );
};

export default PlayerActionsDropdown;
