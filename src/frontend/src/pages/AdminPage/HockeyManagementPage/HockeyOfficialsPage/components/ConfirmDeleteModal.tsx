import { useTranslation } from 'react-i18next';
import type { HockeyOfficialDto } from '../../../../../types/hockey/hockeyTypes';
import './ConfirmDeleteModal.scss';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  official: HockeyOfficialDto | null;
  officialName: string;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

function ConfirmDeleteModal({
  isOpen,
  official,
  officialName,
  onConfirm,
  onCancel,
  isDeleting,
}: ConfirmDeleteModalProps) {
  const { t } = useTranslation();

  if (!isOpen || !official) {
    return null;
  }

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="confirm-delete-modal" onClick={(event) => event.stopPropagation()}>
        <div className="modal-header">
          <h3>{t('hockey.officials.confirmDeactivate.title', 'Confirm Deactivate')}</h3>
        </div>
        <div className="modal-body">
          <p className="warning-text">
            {t(
              'hockey.officials.confirmDeactivate.message',
              'Are you sure you want to deactivate referee {{refereeName}}?',
              { refereeName: officialName },
            )}
          </p>
          <div className="referee-details">
            <div className="detail-item">
              <span className="label">{t('hockey.officials.table.name', 'Name')}:</span>
              <span className="value">{officialName}</span>
            </div>
            <div className="detail-item">
              <span className="label">{t('hockey.officials.role', 'Role')}:</span>
              <span className="value">{t(`hockey.officials.roles.${official.officialRole}`, official.officialRole)}</span>
            </div>
            <div className="detail-item">
              <span className="label">{t('hockey.officials.table.status', 'Status')}:</span>
              <span className={`value status-badge ${official.isActive ? 'active' : 'inactive'}`}>
                {official.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </div>
            <div className="detail-item">
              <span className="label">{t('hockey.officials.table.matchesOfficiated', 'Matches Officiated')}:</span>
              <span className="value">{official.matchesOfficiated}</span>
            </div>
            {official.licenseExpiryDate && (
              <div className="detail-item">
                <span className="label">{t('hockey.officials.table.licenseExpiry', 'License Expiry')}:</span>
                <span className="value">{new Date(official.licenseExpiryDate).toLocaleDateString()}</span>
              </div>
            )}
          </div>
        </div>
        <div className="modal-footer">
          <button type="button" onClick={onCancel} className="cancel-button" disabled={isDeleting}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button type="button" onClick={onConfirm} className="delete-button" disabled={isDeleting}>
            {isDeleting ? t('common.saving', 'Saving...') : t('common.deactivate', 'Deactivate')}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ConfirmDeleteModal;
