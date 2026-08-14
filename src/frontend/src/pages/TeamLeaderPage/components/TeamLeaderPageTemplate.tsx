import { type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../../context/AuthContext';
import LanguageToggle from '../../../components/LanguageToggle/LanguageToggle';
import './TeamLeaderPageTemplate.scss';

interface TeamLeaderPageTemplateProps {
  children: ReactNode;
  /** Optional page title shown under the header. */
  title?: string;
}

function TeamLeaderPageTemplate({ children, title }: TeamLeaderPageTemplateProps) {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/team-leader/login', { replace: true });
  };

  return (
    <div className="team-leader-layout">
      <header className="team-leader-header">
        <div className="team-leader-header-inner">
          <Link to="/team-leader" className="team-leader-brand">
            <span className="team-leader-brand-name">MAHL</span>
            <span className="team-leader-brand-sub">{t('teamLeader.view', 'Team leader view')}</span>
          </Link>
          <div className="team-leader-header-actions">
            <LanguageToggle />
            {user && <span className="team-leader-user">{user.person?.fullName || user.email}</span>}
            <button type="button" className="team-leader-logout" onClick={() => { void handleLogout(); }}>
              {t('teamLeader.logout', 'Log out')}
            </button>
          </div>
        </div>
      </header>
      <main className="team-leader-content">
        {title && <h1 className="team-leader-page-title">{title}</h1>}
        {children}
      </main>
    </div>
  );
}

export default TeamLeaderPageTemplate;
