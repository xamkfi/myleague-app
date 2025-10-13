import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import './ConfirmDeleteModal.scss';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  player: FloorballPlayerDto | null;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
  bulkCount?: number; // For bulk delete operations
}

const ConfirmDeleteModal = ({
  isOpen,
  player,
  onConfirm,
  onCancel,
  isDeleting,
  bulkCount
}: ConfirmDeleteModalProps) => {  
  const { t } = useTranslation();

  if (!isOpen) return null;
  
  const isBulkDelete = bulkCount !== undefined && bulkCount > 0;
  if (!isBulkDelete && !player) return null;

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="confirm-modal-content" onClick={e => e.stopPropagation()}>
        <div className="confirm-modal-header">
          <h2>
            {isBulkDelete 
              ? t('floorball.players.confirmBulkDelete.title', 'Confirm Bulk Deletion')
              : t('floorball.players.confirmDelete.title', 'Confirm Deletion')
            }
          </h2>
          <button
            className="modal-close"
            onClick={onCancel}
            type="button"
            aria-label="Close modal"
            disabled={isDeleting}
          >
            ×
          </button>
        </div>

        <div className="confirm-modal-body">
          <div className="warning-icon">
            ⚠️
          </div>
          <div className="confirm-message">
            <p>
              {isBulkDelete 
                ? t('floorball.players.confirmBulkDelete.message', 
                    'Are you sure you want to delete {{count}} selected player(s)?', 
                    { count: bulkCount }
                  )
                : t('floorball.players.confirmDelete.message', 
                    'Are you sure you want to delete {{playerName}}?', 
                    { playerName: player?.person.fullName }
                  )
              }
            </p>
            <p className="warning-text">
              {isBulkDelete 
                ? t('floorball.players.confirmBulkDelete.warning', 'This action cannot be undone.')
                : t('floorball.players.confirmDelete.warning', 'This action cannot be undone.')
              }
            </p>
          </div>
        </div>

        <div className="confirm-modal-footer">
          <button
            type="button"
            onClick={onCancel}
            className="cancel-button"
            disabled={isDeleting}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            className="delete-button"
            disabled={isDeleting}
          >
            {isDeleting ? 
              (isBulkDelete 
                ? t('floorball.players.actions.bulkDeleting', 'Deleting players...')
                : t('common.deleting', 'Deleting...')
              ) : (
                isBulkDelete 
                  ? t('floorball.players.actions.confirmBulkDelete', 'Delete All')
                  : t('common.delete', 'Delete')
              )
            }
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDeleteModal; 