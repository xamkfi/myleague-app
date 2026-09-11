import { useTranslation } from 'react-i18next';
import type { FootballRefereeDto } from '../../../../../api/football/footballRefereeService';
import './ConfirmDeleteModal.scss';

interface ConfirmDeleteModalProps {
  isOpen: boolean;
  referee: FootballRefereeDto | null;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

const ConfirmDeleteModal = ({
  isOpen,
  referee,
  onConfirm,
  onCancel,
  isDeleting
}: ConfirmDeleteModalProps) => {
  const { t } = useTranslation();

  if (!isOpen || !referee) return null;

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="confirm-delete-modal" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{t('football.referees.confirmDelete.title', 'Confirm Delete')}</h3>
        </div>
        
        <div className="modal-body">
          <p className="warning-text">
            {t('football.referees.confirmDelete.message', 
              'Are you sure you want to delete referee {{refereeName}}? This action cannot be undone.',
              { refereeName: referee.person.fullName }
            )}
          </p>
          
          <div className="referee-details">
            <div className="detail-item">
              <span className="label">{t('football.referees.table.name', 'Name')}:</span>
              <span className="value">{referee.person.fullName}</span>
            </div>
            <div className="detail-item">
              <span className="label">{t('football.referees.table.status', 'Status')}:</span>
              <span className={`value status-badge ${referee.isActive ? 'active' : 'inactive'}`}>
                {referee.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </div>
            <div className="detail-item">
              <span className="label">{t('football.referees.table.matchesOfficiated', 'Matches Officiated')}:</span>
              <span className="value">{referee.matchesOfficiated}</span>
            </div>
            {referee.licenseExpiryDate && (
              <div className="detail-item">
                <span className="label">{t('football.referees.table.licenseExpiry', 'License Expiry')}:</span>
                <span className="value">{new Date(referee.licenseExpiryDate).toLocaleDateString()}</span>
              </div>
            )}
          </div>
        </div>
        
        <div className="modal-footer">
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
              t('common.deleting', 'Deleting...') : 
              t('common.delete', 'Delete')
            }
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDeleteModal; 