import { useTranslation } from 'react-i18next';
import type { SystemUser } from '../../../../types/admin/userTypes';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  user: SystemUser | null;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

const ConfirmDeleteModal = ({
  isOpen,
  user,
  onConfirm,
  onCancel,
  isDeleting,
}: ConfirmDeleteModalProps) => {
  const { t } = useTranslation();

  if (!isOpen || !user) {
    return null;
  }

  return (
    <div className="user-modal__overlay" onClick={onCancel} role="presentation">
      <div
        className="user-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
      >
        <header className="user-modal__header">
          <h3>{t('admin.users.confirmDelete.title', 'Delete user')}</h3>
        </header>

        <section className="user-modal__body">
          <p>
            {t(
              'admin.users.confirmDelete.message',
              'Are you sure you want to delete user "{{userName}}"? This action cannot be undone.',
              { userName: user.person?.fullName ?? user.email },
            )}
          </p>

          <div className="user-modal__details">
            <div>
              <span className="label">
                {t('admin.users.table.person', 'Person')}
              </span>
              <span className="value">{user.person?.fullName ?? '—'}</span>
            </div>
            <div>
              <span className="label">
                {t('admin.users.table.email', 'Email')}
              </span>
              <span className="value">{user.email}</span>
            </div>
            <div>
              <span className="label">
                {t('admin.users.table.status', 'Status')}
              </span>
              <span
                className={`user-status ${user.isActive ? 'user-status--active' : 'user-status--inactive'}`}
              >
                {user.isActive
                  ? t('common.active', 'Active')
                  : t('common.inactive', 'Inactive')}
              </span>
            </div>
          </div>

          <p className="user-modal__warning">
            {t(
              'admin.users.confirmDelete.warning',
              'The person record will not be deleted, only the user account.',
            )}
          </p>
        </section>

        <footer className="user-modal__footer">
          <button
            type="button"
            className="modal-btn modal-btn--secondary"
            onClick={onCancel}
            disabled={isDeleting}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            className="modal-btn modal-btn--danger"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting
              ? t('common.deleting', 'Deleting...')
              : t('common.delete', 'Delete')}
          </button>
        </footer>
      </div>
    </div>
  );
};

export default ConfirmDeleteModal;
