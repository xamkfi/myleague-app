import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './AdminNavBar.scss';
import PersonsIcon from '../../assets/adminIcons/Persons.svg';
import NewsIcon from '../../assets/adminIcons/News.svg';
import SportsIcon from '../../assets/adminIcons/Sports.svg';

function AdminNavBar() {
  const { t } = useTranslation();
  const location = useLocation();
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  return (
    <nav className="admin-navbar">
      <div className="admin-navbar-header">
        <Link to="/admin" className="admin-navbar-brand">
          <h1>MAHL</h1>
        </Link>
        <p className="admin-navbar-subtitle">Admin view</p>
      </div>

      <div className="admin-navbar-content">
        <div className="admin-navbar-section">
          <h3 className="admin-navbar-section-title">Database</h3>
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isActive('/admin') ? 'active' : ''}`}>
              <Link to="/admin">
                <img src={SportsIcon} alt="Home" className="icon" />
                <span>Home</span>
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/persons') ? 'active' : ''}`}>
              <Link to="/admin/persons">
                <img src={PersonsIcon} alt="Persons" className="icon" />
                <span>Persons</span>
              </Link>
            </li>
            <li className={`admin-navbar-item ${isActive('/admin/news') ? 'active' : ''}`}>
              <Link to="/admin/news">
                <img src={NewsIcon} alt="News" className="icon" />
                <span>News</span>
              </Link>
            </li>
          </ul>
        </div>

        <div className="admin-navbar-section">
          <h3 className="admin-navbar-section-title">Sports</h3>
          <ul className="admin-navbar-menu">
            <li className={`admin-navbar-item ${isActive('/admin/floorball') ? 'active' : ''}`}>
              <Link to="/admin/floorball">
                <img src={SportsIcon} alt="Floorball" className="icon" />
                <span>Floorball</span>
              </Link>
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

