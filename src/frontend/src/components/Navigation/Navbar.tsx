import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import LanguageToggle from '../LanguageToggle/LanguageToggle';
import type { Club } from '../../api/common/clubService';
import { getClubs } from '../../api/common/clubService';
import { createClubSlug } from '../../utils/slugUtils';
import './Navbar.scss';
import SearchBar from '../SearchBar';

// Sports configuration
const SPORTS_CONFIG = [
  { id: 'floorball', path: '/sports/floorball', translationKey: 'sports.floorball' },
  { id: 'icehockey', path: '/sports/icehockey', translationKey: 'sports.iceHockey', disabled: true }
];

// Custom hook for mobile detection
const useIsMobile = () => {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const checkIsMobile = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    checkIsMobile();
    window.addEventListener('resize', checkIsMobile);
    return () => window.removeEventListener('resize', checkIsMobile);
  }, []);

  return isMobile;
};


function Navbar() {
  const { t } = useTranslation();
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const dropdownRef = useRef<HTMLLIElement>(null);
  const sportsDropdownRef = useRef<HTMLLIElement>(null);
  const isMobile = useIsMobile();
  
  // Add hamburger menu toggle
  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  // Close mobile menu when clicking outside
  const closeMobileMenu = () => {
    setIsMobileMenuOpen(false);
  };

  // Close mobile menu when window is resized to desktop
  useEffect(() => {
    if (!isMobile && isMobileMenuOpen) {
      setIsMobileMenuOpen(false);
    }
  }, [isMobile, isMobileMenuOpen]);

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
      const target = event.target as Node;
      
      // Check if click is inside any dropdown
      const isInsideClubs = dropdownRef.current?.contains(target);
      const isInsideSports = sportsDropdownRef.current?.contains(target);
      
      // Only close if click is outside ALL dropdowns
      if (!isInsideClubs && !isInsideSports) {
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
        <Link to="/" onClick={closeMobileMenu}>
          <h1>MAHL</h1>
        </Link>
      </div>
      
      {/* Mobile hamburger button */}
      <button 
        className="navbar-mobile-toggle"
        onClick={toggleMobileMenu}
        aria-label="Toggle mobile menu"
        aria-expanded={isMobileMenuOpen}
      >
        <span className={`hamburger-line ${isMobileMenuOpen ? 'open' : ''}`}></span>
        <span className={`hamburger-line ${isMobileMenuOpen ? 'open' : ''}`}></span>
        <span className={`hamburger-line ${isMobileMenuOpen ? 'open' : ''}`}></span>
      </button>
      
      {/* Desktop search bar */}
      <div className="navbar-search desktop-only">
        <SearchBar />
      </div>
      
      {/* Desktop menu */}
      <div className="navbar-menu desktop-only">
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
          <li 
            ref={sportsDropdownRef}
            className={`navbar-item dropdown ${activeDropdown === 'sports' ? 'active' : ''}`}
          >
            <div className="dropdown-trigger" onClick={() => handleDropdownClick('sports')}>
              <span className="dropdown-label">{t('nav.sports')}</span>
              <span className="dropdown-icon">▼</span>
            </div>
            {activeDropdown === 'sports' && (
              <ul className="dropdown-menu">
                <li>
                  <Link to="/sports" onClick={() => setActiveDropdown(null)}>{t('sports.allSports')}</Link>
                </li>
                {SPORTS_CONFIG.map((sport) => (
                  <li key={sport.id}>
                    {sport.disabled ? (
                      <span className="disabled-link">{t(sport.translationKey)}</span>
                    ) : (
                      <Link to={sport.path} onClick={() => setActiveDropdown(null)}>{t(sport.translationKey)}</Link>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </li>
          <li 
            ref={dropdownRef}
            className={`navbar-item dropdown ${activeDropdown === 'clubs' ? 'active' : ''}`}
          >
            <div className="dropdown-trigger" onClick={() => handleDropdownClick('clubs')}>
              <span className="dropdown-label">{t('nav.clubs')}</span>
              <span className="dropdown-icon">▼</span>
            </div>
            {activeDropdown === 'clubs' && (
              <ul className="dropdown-menu">
                {loading ? (
                  <li className="loading">Loading clubs...</li>
                ) : (
                  clubs.map((club) => (
                    <li key={club.id}>
                      <Link to={`/club/${createClubSlug(club)}`} onClick={() => setActiveDropdown(null)}>
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
      
      {/* Desktop language toggle */}
      <div className="navbar-end desktop-only">
        <div className="navbar-language">
          <LanguageToggle />
        </div>
      </div>
      
      {/* Mobile menu overlay */}
      <div className={`navbar-mobile-menu ${isMobileMenuOpen ? 'open' : ''}`}>
        <div className="mobile-menu-content">
          {/* Mobile search bar */}
          <div className="mobile-search">
            <SearchBar />
          </div>
          
          {/* Mobile menu items */}
          <ul className="mobile-navbar-items">
            <li className="mobile-navbar-item">
              <Link to="/uutiset" onClick={closeMobileMenu}>{t('nav.news')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/saannot" onClick={closeMobileMenu}>{t('nav.rules')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/mahl" onClick={closeMobileMenu}>{t('nav.mahl')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/ikaryhmat" onClick={closeMobileMenu}>{t('nav.ageGroups')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/ilmoittaudu" onClick={closeMobileMenu}>{t('nav.register')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/turnaukset" onClick={closeMobileMenu}>{t('nav.tournaments')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <span className="mobile-dropdown-label">{t('nav.sports')}</span>
              <ul className="mobile-sports-list">
                <li>
                  <Link to="/sports" onClick={closeMobileMenu}>{t('sports.allSports')}</Link>
                </li>
                {SPORTS_CONFIG.map((sport) => (
                  <li key={sport.id}>
                    {sport.disabled ? (
                      <span className="disabled-link">{t(sport.translationKey)}</span>
                    ) : (
                      <Link to={sport.path} onClick={closeMobileMenu}>{t(sport.translationKey)}</Link>
                    )}
                  </li>
                ))}
              </ul>
            </li>
            <li className="mobile-navbar-item">
              <span className="mobile-dropdown-label">{t('nav.clubs')}</span>
              {loading ? (
                <div className="mobile-loading">Loading clubs...</div>
              ) : (
                <ul className="mobile-clubs-list">
                  {clubs.map((club) => (
                    <li key={club.id}>
                      <Link to={`/club/${createClubSlug(club)}`} onClick={closeMobileMenu}>
                        {club.name}
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </li>
          </ul>
          
          {/* Mobile language toggle */}
          <div className="mobile-language">
            <LanguageToggle />
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar; 