import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NotFoundPage.scss';

function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('notFoundPage.title')}>
      <div className="not-found-page">
        <p className="not-found-page__code">404</p>
        <h1 className="not-found-page__heading">{t('notFoundPage.heading')}</h1>
        <p className="not-found-page__description">{t('notFoundPage.description')}</p>
        <Link to="/" className="not-found-page__home">
          {t('notFoundPage.home')}
        </Link>
      </div>
    </PageTemplate>
  );
}

export default NotFoundPage;
