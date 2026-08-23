import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import { useInProgressMatches } from '../../hooks/useInProgressMatches';
import { useInProgressFootballMatches } from '../../hooks/useInProgressFootballMatches';
import { useHockeyInProgressMatches } from '../../hooks/useHockeyInProgressMatches';
import LiveDot from '../LiveDot/LiveDot';
import './AdminNavBar.scss';
import RulesIcon from '../../assets/adminIcons/Rules.svg';
import PersonsIcon from '../../assets/adminIcons/Persons.svg';
import NewsIcon from '../../assets/adminIcons/News.svg';
import SportsIcon from '../../assets/adminIcons/Sports.svg';
import TeamsIcon from '../../assets/adminIcons/Teams.svg';
import PlayersIcon from '../../assets/adminIcons/Persons.svg';
import SeasonsIcon from '../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../assets/adminIcons/Referees.svg';
import ClubsIcon from '../../assets/adminIcons/Clubs.svg';
import LeaguesIcon from '../../assets/adminIcons/Leagues.svg';

interface AdminNavBarProps {
  collapsed: boolean;
  onToggleCollapse: () => void;
}

function AdminNavBar({ collapsed, onToggleCollapse }: AdminNavBarProps) {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);
  const [floorballDropdownOpen, setFloorballDropdownOpen] = useState(true);
  const [footballDropdownOpen, setFootballDropdownOpen] = useState(true);
  const [hockeyDropdownOpen, setHockeyDropdownOpen] = useState(true);
  const [siteContentDropdownOpen, setSiteContentDropdownOpen] = useState(true);
  const inProgress = useInProgressMatches();
  const footballInProgress = useInProgressFootballMatches();
  const hockeyLive = useHockeyInProgressMatches();
  const totalLive: number = inProgress.totalCount;
  const seasonLive: number = inProgress.countByCompetitionType.season;
  const tournamentLive: number = inProgress.countByCompetitionType.tournament;
  const footballTotalLive: number = footballInProgress.totalCount;
  const footballSeasonLive: number = footballInProgress.countByCompetitionType.season;
  const footballTournamentLive: number = footballInProgress.countByCompetitionType.tournament;

  const handleLogout = async () => {
    await logout();
    navigate('/admin/login', { replace: true });
  };

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  const isFloorballActive = () => {
    return location.pathname.startsWith('/admin/floorball');
  };

  const isFootballActive = () => {
    return location.pathname.startsWith('/admin/football');
  };

  const isHockeyActive = () => {
    return location.pathname.startsWith('/admin/hockey');
  };

  const isSiteContentActive = () => {
    return location.pathname.startsWith('/admin/site-content');
  };

  const userLabel = user?.person?.fullName?.trim() || user?.email?.trim() || '?';
  const userInitial = userLabel.charAt(0).toUpperCase();

  return (
    <nav className={`admin-navbar ${collapsed ? 'admin-navbar--collapsed' : ''}`}>
      <div className="admin-navbar-header">
        <div className="admin-navbar-header-row">
          <Link to="/admin" className="admin-navbar-brand">
            <h1>{collapsed ? 'M' : 'MAHL'}</h1>
          </Link>
          <button
            className="admin-navbar-toggle"
            onClick={onToggleCollapse}
            aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            <span className={`admin-navbar-toggle-icon ${collapsed ? 'admin-navbar-toggle-icon--collapsed' : ''}`}>
              ‹
            </span>
          </button>
        </div>
        {!collapsed && (
          <p className="admin-navbar-subtitle">{t('admin.view', 'Admin view')}</p>
        )}
      </div>

      <div className="admin-navbar-content">
        <div className="admin-navbar-section">
          {!collapsed && (
            <h3 className="admin-navbar-section-title">{t('admin.database', 'Database')}</h3>
          )}
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isActive('/admin') ? 'active' : ''}`}>
              <Link to="/admin" title={collapsed ? t('admin.actions.home', 'Home') : undefined}>
                <img src={SportsIcon} alt="Home" className="icon" />
                {!collapsed && <span>{t('admin.actions.home', 'Home')}</span>}
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/persons') ? 'active' : ''}`}>
              <Link to="/admin/persons" title={collapsed ? t('admin.actions.persons', 'Persons') : undefined}>
                <img src={PersonsIcon} alt="Persons" className="icon" />
                {!collapsed && <span>{t('admin.actions.persons', 'Persons')}</span>}
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/news') ? 'active' : ''}`}>
              <Link to="/admin/news" title={collapsed ? t('admin.actions.news', 'News') : undefined}>
                <img src={NewsIcon} alt="News" className="icon" />
                {!collapsed && <span>{t('admin.actions.news', 'News')}</span>}
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/divisions') ? 'active' : ''}`}>
              <Link to="/admin/divisions" title={collapsed ? t('admin.actions.divisions', 'Divisions') : undefined}>
                <img src={LeaguesIcon} alt="Divisions" className="icon" />
                {!collapsed && <span>{t('admin.actions.divisions', 'Divisions')}</span>}
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/clubs') ? 'active' : ''}`}>
              <Link to="/admin/clubs" title={collapsed ? t('admin.actions.clubs', 'Clubs') : undefined}>
                <img src={ClubsIcon} alt="Clubs" className="icon" />
                {!collapsed && <span>{t('admin.actions.clubs', 'Clubs')}</span>}
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/users') ? 'active' : ''}`}>
              <Link to="/admin/users" title={collapsed ? t('admin.actions.users', 'System Users') : undefined}>
                <img src={PersonsIcon} alt="Users" className="icon" />
                {!collapsed && <span>{t('admin.actions.users', 'System Users')}</span>}
              </Link>
            </li>
          </ul>
        </div>

        <div className="admin-navbar-section">
          {!collapsed && (
            <h3 className="admin-navbar-section-title">{t('admin.siteContent.title', 'Sivuston sisällöt')}</h3>
          )}
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isSiteContentActive() ? 'active' : ''}`}>
              {collapsed ? (
                <Link to="/admin/site-content/rules" title={t('admin.siteContent.rules', 'Säännöt')}>
                  <img src={RulesIcon} alt="Rules" className="icon" />
                </Link>
              ) : (
                <div className="admin-navbar-dropdown-trigger">
                  <Link
                    to="/admin/site-content/rules"
                    className="admin-navbar-dropdown-trigger-content"
                  >
                    <img src={RulesIcon} alt="Rules" className="icon" />
                    <span>{t('admin.siteContent.titleShort', 'Sisällöt')}</span>
                  </Link>
                  <span
                    className={`admin-navbar-dropdown-arrow ${siteContentDropdownOpen ? 'open' : ''}`}
                    onClick={(e) => {
                      e.stopPropagation();
                      setSiteContentDropdownOpen(!siteContentDropdownOpen);
                    }}
                  >
                    ▼
                  </span>
                </div>
              )}
              {!collapsed && siteContentDropdownOpen && (
                <ul className="admin-navbar-submenu">
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/site-content/info-pages') ? 'active' : ''}`}>
                    <Link to="/admin/site-content/info-pages">
                      {t('admin.siteContent.infoPages.nav', 'MAHL-infosivut')}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/site-content/rules') ? 'active' : ''}`}>
                    <Link to="/admin/site-content/rules">
                      {t('admin.siteContent.rules', 'Säännöt')}
                    </Link>
                  </li>
                </ul>
              )}
            </li>
          </ul>
        </div>

        <div className="admin-navbar-section">
          {!collapsed && (
            <h3 className="admin-navbar-section-title">{t('admin.sportsTitle', 'Sports')}</h3>
          )}
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isFloorballActive() ? 'active' : ''}`}>
              {collapsed ? (
                <Link to="/admin/floorball" title={t('admin.actions.floorball', 'Floorball')}>
                  <span className="admin-navbar-icon-wrapper">
                    <img src={SportsIcon} alt="Floorball" className="icon" />
                    {totalLive > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: totalLive })}
                        className="admin-navbar__live-dot admin-navbar__live-dot--icon-corner"
                      />
                    )}
                  </span>
                </Link>
              ) : (
                <div className="admin-navbar-dropdown-trigger">
                  <Link 
                    to="/admin/floorball" 
                    className="admin-navbar-dropdown-trigger-content"
                  >
                    <img src={SportsIcon} alt="Floorball" className="icon" />
                    <span>{t('admin.actions.floorball', 'Floorball')}</span>
                    {totalLive > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: totalLive })}
                        className="admin-navbar__live-dot"
                      />
                    )}
                  </Link>
                  <span 
                    className={`admin-navbar-dropdown-arrow ${floorballDropdownOpen ? 'open' : ''}`}
                    onClick={(e) => {
                      e.stopPropagation();
                      setFloorballDropdownOpen(!floorballDropdownOpen);
                    }}
                  >
                    ▼
                  </span>
                </div>
              )}
              {!collapsed && floorballDropdownOpen && (
                <ul className="admin-navbar-submenu">
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/teams') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/teams">
                      <img src={TeamsIcon} alt="Teams" className="icon" />
                      <span>{t('floorball.management.actions.teams', 'Teams')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/players') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/players">
                      <img src={PlayersIcon} alt="Players" className="icon" />
                      <span>{t('floorball.management.actions.players', 'Players')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/seasons') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/seasons">
                      <img src={SeasonsIcon} alt="Seasons" className="icon" />
                      <span>{t('floorball.management.actions.seasons', 'Manage Seasons')}</span>
                      {seasonLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: seasonLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/tournaments') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/tournaments">
                      <img src={SeasonsIcon} alt="Tournaments" className="icon" />
                      <span>{t('floorball.management.actions.tournaments', 'Manage Tournaments')}</span>
                      {tournamentLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: tournamentLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/matches') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/matches">
                      <img src={MatchesIcon} alt="Matches" className="icon" />
                      <span>{t('floorball.management.actions.matches', 'Manage Matches')}</span>
                      {totalLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: totalLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/referees') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/referees">
                      <img src={RefereesIcon} alt="Referees" className="icon" />
                      <span>{t('floorball.management.actions.referees', 'Referees')}</span>
                    </Link>
                  </li>
                </ul>
              )}
            </li>
            <li className={`admin-navbar-item ${isFootballActive() ? 'active' : ''}`}>
              {collapsed ? (
                <Link to="/admin/football" title={t('admin.actions.football', 'Football')}>
                  <span className="admin-navbar-icon-wrapper">
                    <img src={SportsIcon} alt="Football" className="icon" />
                    {footballTotalLive > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: footballTotalLive })}
                        className="admin-navbar__live-dot admin-navbar__live-dot--icon-corner"
                      />
                    )}
                  </span>
                </Link>
              ) : (
                <div className="admin-navbar-dropdown-trigger">
                  <Link
                    to="/admin/football"
                    className="admin-navbar-dropdown-trigger-content"
                  >
                    <img src={SportsIcon} alt="Football" className="icon" />
                    <span>{t('admin.actions.football', 'Football')}</span>
                    {footballTotalLive > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: footballTotalLive })}
                        className="admin-navbar__live-dot"
                      />
                    )}
                  </Link>
                  <span
                    className={`admin-navbar-dropdown-arrow ${footballDropdownOpen ? 'open' : ''}`}
                    onClick={(e) => {
                      e.stopPropagation();
                      setFootballDropdownOpen(!footballDropdownOpen);
                    }}
                  >
                    ▼
                  </span>
                </div>
              )}
              {!collapsed && footballDropdownOpen && (
                <ul className="admin-navbar-submenu">
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/teams') ? 'active' : ''}`}>
                    <Link to="/admin/football/teams">
                      <img src={TeamsIcon} alt="Teams" className="icon" />
                      <span>{t('football.management.actions.teams', 'Teams')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/players') ? 'active' : ''}`}>
                    <Link to="/admin/football/players">
                      <img src={PlayersIcon} alt="Players" className="icon" />
                      <span>{t('football.management.actions.players', 'Players')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/seasons') ? 'active' : ''}`}>
                    <Link to="/admin/football/seasons">
                      <img src={SeasonsIcon} alt="Seasons" className="icon" />
                      <span>{t('football.management.actions.seasons', 'Manage Seasons')}</span>
                      {footballSeasonLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: footballSeasonLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/tournaments') ? 'active' : ''}`}>
                    <Link to="/admin/football/tournaments">
                      <img src={SeasonsIcon} alt="Tournaments" className="icon" />
                      <span>{t('football.management.actions.tournaments', 'Manage Tournaments')}</span>
                      {footballTournamentLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: footballTournamentLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/matches') ? 'active' : ''}`}>
                    <Link to="/admin/football/matches">
                      <img src={MatchesIcon} alt="Matches" className="icon" />
                      <span>{t('football.management.actions.matches', 'Manage Matches')}</span>
                      {footballTotalLive > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: footballTotalLive })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/football/referees') ? 'active' : ''}`}>
                    <Link to="/admin/football/referees">
                      <img src={RefereesIcon} alt="Referees" className="icon" />
                      <span>{t('football.management.actions.referees', 'Referees')}</span>
                    </Link>
                  </li>
                </ul>
              )}
            </li>
            <li className={`admin-navbar-item ${isHockeyActive() ? 'active' : ''}`}>
              {collapsed ? (
                <Link to="/admin/hockey" title={t('admin.actions.hockey', 'Ice hockey')}>
                  <span className="admin-navbar-icon-wrapper">
                    <img src={SportsIcon} alt="Hockey" className="icon" />
                    {hockeyLive.totalCount > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: hockeyLive.totalCount })}
                        className="admin-navbar__live-dot admin-navbar__live-dot--icon-corner"
                      />
                    )}
                  </span>
                </Link>
              ) : (
                <div className="admin-navbar-dropdown-trigger">
                  <Link to="/admin/hockey" className="admin-navbar-dropdown-trigger-content">
                    <img src={SportsIcon} alt="Hockey" className="icon" />
                    <span>{t('admin.actions.hockey', 'Ice hockey')}</span>
                    {hockeyLive.totalCount > 0 && (
                      <LiveDot
                        tone="dark"
                        ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: hockeyLive.totalCount })}
                        className="admin-navbar__live-dot"
                      />
                    )}
                  </Link>
                  <span
                    className={`admin-navbar-dropdown-arrow ${hockeyDropdownOpen ? 'open' : ''}`}
                    onClick={(event) => {
                      event.stopPropagation();
                      setHockeyDropdownOpen(!hockeyDropdownOpen);
                    }}
                  >
                    ▼
                  </span>
                </div>
              )}
              {!collapsed && hockeyDropdownOpen && (
                <ul className="admin-navbar-submenu">
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/teams') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/teams">
                      <img src={TeamsIcon} alt="Teams" className="icon" />
                      <span>{t('hockey.management.actions.teams', 'Teams')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/players') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/players">
                      <img src={PlayersIcon} alt="Players" className="icon" />
                      <span>{t('hockey.management.actions.players', 'Players')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/seasons') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/seasons">
                      <img src={SeasonsIcon} alt="Seasons" className="icon" />
                      <span>{t('hockey.management.actions.seasons', 'Seasons')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/tournaments') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/tournaments">
                      <img src={SeasonsIcon} alt="Tournaments" className="icon" />
                      <span>{t('hockey.management.actions.tournaments', 'Tournaments')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/matches') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/matches">
                      <img src={MatchesIcon} alt="Matches" className="icon" />
                      <span>{t('hockey.management.actions.matches', 'Matches')}</span>
                      {hockeyLive.totalCount > 0 && (
                        <LiveDot
                          tone="dark"
                          ariaLabel={t('admin.navbar.matchesInProgress', '{{count}} match(es) in progress', { count: hockeyLive.totalCount })}
                          className="admin-navbar__live-dot"
                        />
                      )}
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/hockey/officials') ? 'active' : ''}`}>
                    <Link to="/admin/hockey/officials">
                      <img src={RefereesIcon} alt="Officials" className="icon" />
                      <span>{t('hockey.management.actions.officials', 'Officials')}</span>
                    </Link>
                  </li>
                </ul>
              )}
            </li>
          </ul>
        </div>
      </div>

      <div className="admin-navbar-footer">
        {collapsed ? (
          <div
            className="admin-navbar-user-avatar"
            onClick={() => setUserDropdownOpen(!userDropdownOpen)}
            title={user?.person?.fullName ?? user?.email ?? ''}
          >
            {userInitial}
          </div>
        ) : (
          <div 
            className={`admin-navbar-user ${userDropdownOpen ? 'open' : ''}`}
            onClick={() => setUserDropdownOpen(!userDropdownOpen)}
          >
            <div className="admin-navbar-user-info">
              <span className="admin-navbar-user-name">
                {user?.person?.fullName ?? user?.email ?? '—'}
              </span>
              <span className="admin-navbar-user-role">{user?.email ?? ''}</span>
            </div>
            <span className="admin-navbar-user-dropdown-icon">▼</span>
          </div>
        )}
        {userDropdownOpen && (
          <div className="admin-navbar-user-menu">
            <button className="admin-navbar-user-menu-item" onClick={handleLogout}>
              {t('auth.logout', 'Log out')}
            </button>
          </div>
        )}
      </div>
    </nav>
  );
}

export default AdminNavBar;

