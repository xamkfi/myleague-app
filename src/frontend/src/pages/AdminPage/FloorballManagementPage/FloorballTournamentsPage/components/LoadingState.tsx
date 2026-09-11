import { useTranslation } from 'react-i18next';

export const LoadingState = () => {
  const { t } = useTranslation();

  return (
    <div className="floorball-tournaments-loading">
      <p>{t('common.loading', 'Loading...')}</p>
    </div>
  );
};
