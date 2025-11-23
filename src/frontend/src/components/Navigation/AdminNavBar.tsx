import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './AdminNavBar.scss';
import PersonsIcon from '../../assets/adminIcons/Persons.svg';
import NewsIcon from '../../assets/adminIcons/News.svg';
import SportsIcon from '../../assets/adminIcons/Sports.svg';
import TeamsIcon from '../../assets/adminIcons/Teams.svg';
import PlayersIcon from '../../assets/adminIcons/Persons.svg';
import SeasonsIcon from '../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../assets/adminIcons/Referees.svg';
import ClubsIcon from '../../assets/adminIcons/Clubs.svg';

function AdminNavBar() {
  const { t } = useTranslation();
  const location = useLocation();
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);
  const [floorballDropdownOpen, setFloorballDropdownOpen] = useState(true);

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  const isFloorballActive = () => {
    return location.pathname.startsWith('/admin/floorball');
  };

  return (
    <nav className="admin-navbar">
      <div className="admin-navbar-header">
        <Link to="/admin" className="admin-navbar-brand">
          <h1>MAHL</h1>
        </Link>
        <p className="admin-navbar-subtitle">{t('admin.view', 'Admin view')}</p>
      </div>

      <div className="admin-navbar-content">
        <div className="admin-navbar-section">
          <h3 className="admin-navbar-section-title">{t('admin.database', 'Database')}</h3>
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isActive('/admin') ? 'active' : ''}`}>
              <Link to="/admin">
                <img src={SportsIcon} alt="Home" className="icon" />
                <span>{t('admin.actions.home', 'Home')}</span>
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/persons') ? 'active' : ''}`}>
              <Link to="/admin/persons">
                <img src={PersonsIcon} alt="Persons" className="icon" />
                <span>{t('admin.actions.persons', 'Persons')}</span>
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/news') ? 'active' : ''}`}>
              <Link to="/admin/news">
                <img src={NewsIcon} alt="News" className="icon" />
                <span>{t('admin.actions.news', 'News')}</span>
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/clubs') ? 'active' : ''}`}>
              <Link to="/admin/clubs">
                <img src={ClubsIcon} alt="Clubs" className="icon" />
                <span>{t('admin.actions.clubs', 'Clubs')}</span>
              </Link>
            </li>
          </ul>
        </div>

        <div className="admin-navbar-section">
          <h3 className="admin-navbar-section-title">{t('admin.sportsTitle', 'Sports')}</h3>
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isFloorballActive() ? 'active' : ''}`}>
              <div className="admin-navbar-dropdown-trigger">
                <Link 
                  to="/admin/floorball" 
                  className="admin-navbar-dropdown-trigger-content"
                >
                  <img src={SportsIcon} alt="Floorball" className="icon" />
                  <span>{t('admin.actions.floorball', 'Floorball')}</span>
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
              {floorballDropdownOpen && (
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
                      <span>{t('floorball.management.actions.seasons', 'Seasons')}</span>
                    </Link>
                  </li>
                  <li className={`admin-navbar-submenu-item ${isActive('/admin/floorball/matches') ? 'active' : ''}`}>
                    <Link to="/admin/floorball/matches">
                      <img src={MatchesIcon} alt="Matches" className="icon" />
                      <span>{t('floorball.management.actions.matches', 'Matches')}</span>
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
          </ul>
        </div>
      </div>

      <div className="admin-navbar-footer">
        <div 
          className={`admin-navbar-user ${userDropdownOpen ? 'open' : ''}`}
          onClick={() => setUserDropdownOpen(!userDropdownOpen)}
        >
          <div className="admin-navbar-user-info">
            <span className="admin-navbar-user-name">MIKKO</span>
            <span className="admin-navbar-user-role">Super Admin</span>
          </div>
          <span className="admin-navbar-user-dropdown-icon">▼</span>
        </div>
      </div>
    </nav>
  );
}

export default AdminNavBar;

