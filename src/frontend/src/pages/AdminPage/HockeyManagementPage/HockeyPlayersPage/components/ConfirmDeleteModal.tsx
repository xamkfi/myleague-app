import { useTranslation } from 'react-i18next';
import './ConfirmDeleteModal.scss';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  name: string | null;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
  bulkCount?: number;
}

function ConfirmDeleteModal({ isOpen, name, onConfirm, onCancel, isDeleting, bulkCount }: ConfirmDeleteModalProps) {
  const { t } = useTranslation();
  if (!isOpen) {
    return null;
  }
  const isBulk = Boolean(bulkCount && bulkCount > 0);
  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="confirm-modal-content" onClick={(event) => event.stopPropagation()}>
        <div className="confirm-modal-header">
          <h2>
            {isBulk
              ? t('hockey.players.confirmBulkDelete.title', 'Confirm Bulk Deletion')
              : t('hockey.players.confirmDelete.title', 'Confirm Deletion')}
          </h2>
          <button type="button" className="modal-close" onClick={onCancel}>×</button>
        </div>
        <p>
          {isBulk
            ? t('hockey.players.confirmBulkDelete.message', 'Remove {{count}} players from their teams?', { count: bulkCount })
            : t('hockey.players.confirmDelete.message', 'Remove {{name}} from the team?', { name })}
        </p>
        <div className="confirm-modal-actions">
          <button type="button" onClick={onCancel} disabled={isDeleting}>{t('common.cancel', 'Cancel')}</button>
          <button type="button" className="danger" onClick={onConfirm} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ConfirmDeleteModal;
