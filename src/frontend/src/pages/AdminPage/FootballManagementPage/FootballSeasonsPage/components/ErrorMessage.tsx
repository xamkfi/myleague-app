import './ErrorMessage.scss';

interface ErrorMessageProps {
  message: string;
  type?: 'error' | 'success' | 'warning';
}

export const ErrorMessage = ({ message, type = 'error' }: ErrorMessageProps) => {
  const getIconClass = () => {
    switch (type) {
      case 'success':
        return 'fas fa-check-circle';
      case 'warning':
        return 'fas fa-exclamation-triangle';
      default:
        return 'fas fa-exclamation-circle';
    }
  };

  return (
    <div className={`error-message ${type}`}>
      <i className={getIconClass()}></i>
      <p>{message}</p>
    </div>
  );
}; 