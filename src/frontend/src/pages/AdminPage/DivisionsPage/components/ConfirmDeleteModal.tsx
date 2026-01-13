import { useTranslation } from 'react-i18next';
import type { DivisionType } from '../../../../types/common/divisionType';
import './ConfirmDeleteModal.scss';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  division: DivisionType | null;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

const ConfirmDeleteModal = ({
  isOpen,
  division,
  onConfirm,
  onCancel,
  isDeleting,
}: ConfirmDeleteModalProps) => {
  const { t } = useTranslation();

  if (!isOpen || !division) {
    return null;
  }

  return (
    <div className="division-modal__overlay" onClick={onCancel} role="presentation">
      <div className="division-modal" onClick={(event) => event.stopPropagation()} role="dialog" aria-modal="true">
        <header className="division-modal__header">
          <h3>{t('admin.divisions.confirmDelete.title', 'Delete division')}</h3>
        </header>

        <section className="division-modal__body">
          <p>
            {t(
              'admin.divisions.confirmDelete.message',
              'Are you sure you want to delete division "{{divisionName}}"? This action cannot be undone.',
              { divisionName: division.name },
            )}
          </p>

          <div className="division-modal__details">
            <div>
              <span className="label">{t('admin.divisions.table.sport', 'Sport')}</span>
              <span className="value">{division.sportType}</span>
            </div>
            <div>
              <span className="label">{t('admin.divisions.table.level', 'Level')}</span>
              <span className="value">{division.level}</span>
            </div>
            <div>
              <span className="label">{t('admin.divisions.table.status', 'Status')}</span>
              <span
                className={`division-status ${division.isActive ? 'division-status--active' : 'division-status--inactive'}`}
              >
                {division.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </div>
          </div>
        </section>

        <footer className="division-modal__footer">
          <button type="button" className="modal-btn modal-btn--secondary" onClick={onCancel} disabled={isDeleting}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button type="button" className="modal-btn modal-btn--danger" onClick={onConfirm} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </footer>
      </div>
    </div>
  );
};

export default ConfirmDeleteModal;

