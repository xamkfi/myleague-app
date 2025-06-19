import { useTranslation } from 'react-i18next';
import './LoadingState.scss';

export const LoadingState = () => {
  const { t } = useTranslation();

  return (
    <div className="floorball-seasons-loading">
      <p>{t('common.loading', 'Loading...')}</p>
    </div>
  );
}; 