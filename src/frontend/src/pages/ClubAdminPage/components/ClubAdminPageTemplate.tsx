import { type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../../context/AuthContext';
import LanguageToggle from '../../../components/LanguageToggle/LanguageToggle';
import './ClubAdminPageTemplate.scss';

interface ClubAdminPageTemplateProps {
  children: ReactNode;
  /** Optional page title shown under the header. */
  title?: string;
}

function ClubAdminPageTemplate({ children, title }: ClubAdminPageTemplateProps) {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/club-admin/login', { replace: true });
  };

  return (
    <div className="club-admin-layout">
      <header className="club-admin-header">
        <div className="club-admin-header-inner">
          <Link to="/club-admin" className="club-admin-brand">
            <span className="club-admin-brand-name">MAHL</span>
            <span className="club-admin-brand-sub">{t('clubAdmin.view', 'Club admin view')}</span>
          </Link>
          <div className="club-admin-header-actions">
            <LanguageToggle />
            {user && <span className="club-admin-user">{user.person?.fullName || user.email}</span>}
            <button type="button" className="club-admin-logout" onClick={() => { void handleLogout(); }}>
              {t('clubAdmin.logout', 'Log out')}
            </button>
          </div>
        </div>
      </header>
      <main className="club-admin-content">
        {title && <h1 className="club-admin-page-title">{title}</h1>}
        {children}
      </main>
    </div>
  );
}

export default ClubAdminPageTemplate;
