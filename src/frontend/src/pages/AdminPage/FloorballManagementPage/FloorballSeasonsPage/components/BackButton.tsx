import { useTranslation } from 'react-i18next';
import './BackButton.scss';

interface BackButtonProps {
  onBack: () => void;
}

export const BackButton = ({ onBack }: BackButtonProps) => {
  const { t } = useTranslation();

  return (
    <div className="back-button-container">
      <button
        className="back-button"
        onClick={onBack}
      >
        {t('common.back', 'Back to Floorball Management')}
      </button>
    </div>
  );
}; 