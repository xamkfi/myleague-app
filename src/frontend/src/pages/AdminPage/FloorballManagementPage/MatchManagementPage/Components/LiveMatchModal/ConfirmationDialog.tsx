import React from 'react';

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

const ConfirmationDialog: React.FC<ConfirmationDialogProps> = ({
  isOpen,
  icon,
  title,
  message,
  warningMessage,
  confirmText,
  cancelText = 'Cancel',
  isLoading = false,
  onConfirm,
  onCancel
}) => {
  if (!isOpen) return null;

  return (
    <div className="confirmation-dialog-overlay">
      <div className="confirmation-dialog">
        <div className="confirmation-header">
          <span className="confirmation-icon">{icon}</span>
          <h3>{title}</h3>
        </div>
        <div className="confirmation-content">
          <p>{message}</p>
          {warningMessage && (
            <p className="confirmation-warning">{warningMessage}</p>
          )}
        </div>
        <div className="confirmation-actions">
          <button 
            onClick={onConfirm} 
            className="confirm-btn"
            disabled={isLoading}
          >
            {isLoading ? 'Processing...' : confirmText}
          </button>
          <button 
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