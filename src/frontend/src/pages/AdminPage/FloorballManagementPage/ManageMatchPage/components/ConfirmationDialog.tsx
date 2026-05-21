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
  cancelText = 'Cancel',
  isLoading = false,
  onConfirm,
  onCancel,
}: ConfirmationDialogProps) => {

  // Do not render dialog when closed.
  if (!isOpen) return null;

  // Prevent closing while request is processing.
  const handleOverlayClick = () => {
    if (!isLoading) {
      onCancel();
    }
  };

  return (
    <div
      className="confirmation-dialog-overlay"
      onClick={handleOverlayClick}
    >
      <div
        className="confirmation-dialog"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="confirmation-header">
          <span className="confirmation-icon">{icon}</span>
          <h3>{title}</h3>
        </div>

        <div className="confirmation-content">
          <p>{message}</p>

          {/* Show warning text only if provided */}
          {warningMessage && (
            <p className="confirmation-warning">
              {warningMessage}
            </p>
          )}
        </div>

        <div className="confirmation-actions">
          <button
            type="button"
            onClick={onConfirm}
            className="confirm-btn"
            disabled={isLoading}
          >
            {/* Show loading text during API request */}
            {isLoading ? 'Processing...' : confirmText}
          </button>

          <button
            type="button"
            onClick={onCancel}
            className="cancel-btn"
            disabled={isLoading}
          >
            {cancelText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmationDialog;