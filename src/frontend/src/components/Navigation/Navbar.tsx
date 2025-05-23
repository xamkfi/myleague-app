import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import LanguageToggle from '../LanguageToggle';
import './Navbar.scss';

interface NavbarProps {
  onLogin?: () => void;
}

function Navbar({ onLogin }: NavbarProps) {
  const { t } = useTranslation();
  
  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/">
          <h1>MAHL</h1>
        </Link>
      </div>
      <div className="navbar-menu">
        <ul className="navbar-items">
          <li className="navbar-item">
            <Link to="/uutiset">{t('nav.news')}</Link>
          </li>
          <li className="navbar-item">
            <Link to="/saannot">{t('nav.rules')}</Link>
          </li>
          <li className="navbar-item dropdown">
            <Link to="/mahl">{t('nav.mahl')}</Link>
            <span className="dropdown-icon">▼</span>
          </li>
          <li className="navbar-item dropdown">
            <Link to="/ikaryhmat">{t('nav.ageGroups')}</Link>
            <span className="dropdown-icon">▼</span>
          </li>
          <li className="navbar-item">
            <Link to="/ilmoittaudu">{t('nav.register')}</Link>
          </li>
          <li className="navbar-item dropdown">
            <Link to="/turnaukset">{t('nav.tournaments')}</Link>
            <span className="dropdown-icon">▼</span>
          </li>
          <li className="navbar-item dropdown">
            <Link to="/lajit">{t('nav.sports')}</Link>
            <span className="dropdown-icon">▼</span>
          </li>
        </ul>
      </div>
      <div className="navbar-end">
        <div className="navbar-language">
          <LanguageToggle />
        </div>
        <button className="button button-primary" onClick={onLogin}>
          Somelinkit
        </button>
      </div>
    </nav>
  );
}

export default Navbar; 