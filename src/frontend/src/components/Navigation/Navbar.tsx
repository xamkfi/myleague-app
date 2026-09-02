import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import LanguageToggle from '../LanguageToggle/LanguageToggle';
import './Navbar.scss';
import SearchBar from '../SearchBar';
import { MAHL_INFO_PAGES } from '../../constants/mahlInfoPages';
import AudienceSwitcher from '../AudienceSwitcher/AudienceSwitcher';
import SportIcon, { type SportIconSport } from '../SportIcon/SportIcon';
import mahlLogo from '../../assets/logos/Mahl_primary_V3.svg';

interface NavbarSportLink {
  id: SportIconSport;
  path: string;
  translationKey: string;
  disabled?: boolean;
}

const SPORTS_CONFIG: NavbarSportLink[] = [
  { id: 'floorball', path: '/sports/floorball', translationKey: 'sports.floorball' },
  { id: 'football', path: '/sports/football', translationKey: 'sports.football' },
  { id: 'icehockey', path: '/sports/icehockey', translationKey: 'sports.iceHockey' },
];

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
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const sportsDropdownRef = useRef<HTMLLIElement>(null);
  const mahlDropdownRef = useRef<HTMLLIElement>(null);
  const isMobile = useIsMobile();

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  const closeMobileMenu = () => {
    setIsMobileMenuOpen(false);
  };

  useEffect(() => {
    if (!isMobile && isMobileMenuOpen) {
      setIsMobileMenuOpen(false);
    }
  }, [isMobile, isMobileMenuOpen]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      const target = event.target as Node;

      const isInsideSports = sportsDropdownRef.current?.contains(target);
      const isInsideMahl = mahlDropdownRef.current?.contains(target);

      if (!isInsideSports && !isInsideMahl) {
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
        <Link to="/" onClick={closeMobileMenu} className="navbar-brand-logo-link">
          <img src={mahlLogo} alt="MAHL" className="navbar-brand-logo" />
        </Link>
        <AudienceSwitcher />
      </div>

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

      <div className="navbar-search desktop-only">
        {!isMobile && <SearchBar />}
      </div>

      <div className="navbar-menu desktop-only">
        <ul className="navbar-items">
          <li className="navbar-item">
            <Link to="/uutiset">{t('nav.news')}</Link>
          </li>
          <li className="navbar-item">
            <Link to="/tapahtumakalenteri">{t('nav.eventCalendar')}</Link>
          </li>
          <li
            ref={mahlDropdownRef}
            className={`navbar-item dropdown ${activeDropdown === 'mahl' ? 'active' : ''}`}
          >
            <div className="dropdown-trigger" onClick={() => handleDropdownClick('mahl')}>
              <span className="dropdown-label">{t('nav.mahl')}</span>
              <span className="dropdown-icon">▼</span>
            </div>
            {activeDropdown === 'mahl' && (
              <ul className="dropdown-menu">
                {MAHL_INFO_PAGES.map((page) => (
                  <li key={page.path}>
                    <Link to={page.path} onClick={() => setActiveDropdown(null)}>
                      {t(page.labelKey, page.defaultLabel)}
                    </Link>
                  </li>
                ))}
                <li>
                  <Link to="/saannot" onClick={() => setActiveDropdown(null)}>{t('nav.rules')}</Link>
                </li>
              </ul>
            )}
          </li>
          <li className="navbar-item dropdown">
            <Link to="/ikaryhmat">{t('nav.ageGroups')}</Link>
            <span className="dropdown-icon">▼</span>
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
                {SPORTS_CONFIG.map((sport: NavbarSportLink) => (
                  <li key={sport.id}>
                    {sport.disabled ? (
                      <span className="disabled-link navbar-sport-link">
                        <SportIcon sport={sport.id} size="sm" decorative />
                        {t(sport.translationKey)}
                      </span>
                    ) : (
                      <Link
                        to={sport.path}
                        onClick={() => setActiveDropdown(null)}
                        className="navbar-sport-link"
                      >
                        <SportIcon sport={sport.id} size="sm" decorative />
                        {t(sport.translationKey)}
                      </Link>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </li>
          <li className="navbar-item">
            <Link to="/clubs">{t('nav.clubs')}</Link>
          </li>
        </ul>
      </div>

      <div className="navbar-end desktop-only">
        <div className="navbar-language">
          <LanguageToggle />
        </div>
      </div>

      <div className={`navbar-mobile-menu ${isMobileMenuOpen ? 'open' : ''}`}>
        <div className="mobile-menu-content">
          <div className="mobile-audience">
            <span className="mobile-audience-label">{t('audience.switcherLabel')}</span>
            <AudienceSwitcher variant="block" />
          </div>

          <div className="mobile-search">
            {isMobile && <SearchBar />}
          </div>

          <ul className="mobile-navbar-items">
            <li className="mobile-navbar-item">
              <Link to="/uutiset" onClick={closeMobileMenu}>{t('nav.news')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/tapahtumakalenteri" onClick={closeMobileMenu}>{t('nav.eventCalendar')}</Link>
            </li>
            <li className="mobile-navbar-item">
              <span className="mobile-dropdown-label">{t('nav.mahl')}</span>
              <ul className="mobile-sports-list">
                {MAHL_INFO_PAGES.map((page) => (
                  <li key={page.path}>
                    <Link to={page.path} onClick={closeMobileMenu}>
                      {t(page.labelKey, page.defaultLabel)}
                    </Link>
                  </li>
                ))}
                <li>
                  <Link to="/saannot" onClick={closeMobileMenu}>{t('nav.rules')}</Link>
                </li>
              </ul>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/ikaryhmat" onClick={closeMobileMenu}>{t('nav.ageGroups')}</Link>
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
                {SPORTS_CONFIG.map((sport: NavbarSportLink) => (
                  <li key={sport.id}>
                    {sport.disabled ? (
                      <span className="disabled-link navbar-sport-link">
                        <SportIcon sport={sport.id} size="sm" inverted decorative />
                        {t(sport.translationKey)}
                      </span>
                    ) : (
                      <Link
                        to={sport.path}
                        onClick={closeMobileMenu}
                        className="navbar-sport-link"
                      >
                        <SportIcon sport={sport.id} size="sm" inverted decorative />
                        {t(sport.translationKey)}
                      </Link>
                    )}
                  </li>
                ))}
              </ul>
            </li>
            <li className="mobile-navbar-item">
              <Link to="/clubs" onClick={closeMobileMenu}>{t('nav.clubs')}</Link>
            </li>
          </ul>

          <div className="mobile-language">
            <LanguageToggle />
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
