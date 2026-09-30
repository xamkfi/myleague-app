import { useTranslation } from 'react-i18next';
import './ConfirmationDialog.scss';

interface ConfirmationDialogProps {
  isOpen: boolean;
  icon: string;
  title: string;
  message: string;
  warningMessage?: string;
  confirmText: string;
  cancelText?: string;
  isLoading?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

const ConfirmationDialog = ({
  isOpen,
  icon,
  title,
  message,
  warningMessage,
  confirmText,
  cancelText,
  isLoading = false,
  onConfirm,
  onCancel
}: ConfirmationDialogProps) => {
  const { t } = useTranslation();
  if (!isOpen) return null;

  return (
    <div className="confirmation-dialog-overlay" onClick={onCancel}>
      <div
        className="confirmation-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirmation-dialog-title"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="confirmation-header">
          <span className="confirmation-icon" aria-hidden="true">{icon}</span>
          <h3 id="confirmation-dialog-title">{title}</h3>
        </div>
        <div className="confirmation-content">
          <p>{message}</p>
          {warningMessage && (
            <p className="confirmation-warning">{warningMessage}</p>
          )}
        </div>
        <div className="confirmation-actions">
          <button
            type="button"
            onClick={onConfirm}
            className="confirm-btn"
            disabled={isLoading}
          >
            {isLoading ? t('common.processing', 'Processing...') : confirmText}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="cancel-btn"
            disabled={isLoading}
          >
            {cancelText ?? t('common.cancel', 'Cancel')}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmationDialog;
