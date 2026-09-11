import './ConfirmationDialog.scss';

/**
 * Shared confirmation dialog. Used by hockey now.
 * We can migrate floorball pages to this component later.
 */
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

function ConfirmationDialog({
  isOpen,
  icon,
  title,
  message,
  warningMessage,
  confirmText,
  cancelText = 'Cancel',
  isLoading = false,
  onConfirm,
  onCancel,
}: ConfirmationDialogProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="confirmation-dialog-overlay" onClick={onCancel}>
      <div className="confirmation-dialog" onClick={(event) => event.stopPropagation()}>
        <div className="confirmation-header">
          <span className="confirmation-icon">{icon}</span>
          <h3>{title}</h3>
        </div>
        <div className="confirmation-content">
          <p>{message}</p>
          {warningMessage && <p className="confirmation-warning">{warningMessage}</p>}
        </div>
        <div className="confirmation-actions">
          <button type="button" onClick={onConfirm} className="confirm-btn" disabled={isLoading}>
            {isLoading ? 'Processing...' : confirmText}
          </button>
          <button type="button" onClick={onCancel} className="cancel-btn" disabled={isLoading}>
            {cancelText}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ConfirmationDialog;
