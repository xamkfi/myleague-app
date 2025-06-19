import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import LanguageToggle from '../LanguageToggle/LanguageToggle';
import type { Club } from '../../api/clubService';
import { getClubs } from '../../api/clubService';
import { createClubSlug } from '../../utils/slugUtils';
import './Navbar.scss';

interface NavbarProps {
  onLogin?: () => void;
}

function Navbar({ onLogin }: NavbarProps) {
  const { t } = useTranslation();
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(false);
  const dropdownRef = useRef<HTMLLIElement>(null);

  useEffect(() => {
    const fetchClubs = async () => {
      try {
        setLoading(true);
        const clubsData = await getClubs();
        setClubs(clubsData);
      } catch (error) {
        console.error('Failed to fetch clubs:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchClubs();
  }, []);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setActiveDropdown(null);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleDropdownClick = (dropdownName: string) => {
    setActiveDropdown(activeDropdown === dropdownName ? null : dropdownName);
  };

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
          <li 
            ref={dropdownRef}
            className={`navbar-item dropdown ${activeDropdown === 'clubs' ? 'active' : ''}`}
            onClick={() => handleDropdownClick('clubs')}
          >
            <span className="dropdown-label">{t('nav.clubs')}</span>
            <span className="dropdown-icon">▼</span>
            {activeDropdown === 'clubs' && (
              <ul className="dropdown-menu">
                {loading ? (
                  <li className="loading">Loading clubs...</li>
                ) : (
                  clubs.map((club) => (
                    <li key={club.id}>
                      <Link to={`/club/${createClubSlug(club)}`}>
                        {club.name}
                      </Link>
                    </li>
                  ))
                )}
              </ul>
            )}
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