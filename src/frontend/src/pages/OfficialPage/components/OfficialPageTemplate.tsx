import { useEffect, type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../../context/AuthContext';
import LanguageToggle from '../../../components/LanguageToggle/LanguageToggle';
import './OfficialPageTemplate.scss';

interface OfficialPageTemplateProps {
  children: ReactNode;
  /** Optional page heading shown above the content. */
  title?: string;
  /** Browser tab title. Falls back to `title`. */
  documentTitle?: string;
  /** Use the full viewport width, e.g. for the live match desk. */
  wide?: boolean;
}

/** Shell for the match official area: header with brand, language, user, and logout. */
function OfficialPageTemplate({ children, title, documentTitle, wide = false }: OfficialPageTemplateProps) {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const tabTitle: string | undefined = documentTitle ?? title;

  useEffect(() => {
    if (!tabTitle) return undefined;
    document.title = `${tabTitle} - MAHL`;
    return () => {
      document.title = 'MAHL';
    };
  }, [tabTitle]);

  const handleLogout = async (): Promise<void> => {
    await logout();
    navigate('/official/login', { replace: true });
  };

  return (
    <div className="official-layout">
      <header className="official-header">
        <div className={`official-header__inner${wide ? ' official-header__inner--wide' : ''}`}>
          <Link to="/official" className="official-header__brand">
            <span className="official-header__brand-name">MAHL</span>
            <span className="official-header__brand-sub">{t('official.view', 'Match official')}</span>
          </Link>
          <div className="official-header__actions">
            <LanguageToggle />
            {user && <span className="official-header__user">{user.person?.fullName || user.email}</span>}
            <button type="button" className="official-header__logout" onClick={() => { void handleLogout(); }}>
              {t('official.logout', 'Log out')}
            </button>
          </div>
        </div>
      </header>
      <main className={`official-content${wide ? ' official-content--wide' : ''}`}>
        {title && <h1 className="official-content__title">{title}</h1>}
        {children}
      </main>
    </div>
  );
}

export default OfficialPageTemplate;
