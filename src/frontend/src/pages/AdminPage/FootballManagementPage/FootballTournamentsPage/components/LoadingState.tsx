import { useTranslation } from 'react-i18next';

export const LoadingState = () => {
  const { t } = useTranslation();

  return (
    <div className="football-tournaments-loading">
      <p>{t('common.loading', 'Loading...')}</p>
    </div>
  );
};
