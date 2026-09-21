import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NotFoundPage.scss';

function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('notFound.title', 'Sivua ei löytynyt')}>
      <div className="not-found-page">
        <p className="not-found-page__code">404</p>
        <h1 className="not-found-page__title">{t('notFound.title', 'Sivua ei löytynyt')}</h1>
        <p className="not-found-page__text">
          {t('notFound.message', 'Etsimääsi sivua ei ole olemassa tai se on siirretty.')}
        </p>
        <Link to="/" className="not-found-page__link">
          {t('notFound.home', 'Etusivulle')}
        </Link>
      </div>
    </PageTemplate>
  );
}

export default NotFoundPage;
