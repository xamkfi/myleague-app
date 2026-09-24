import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { fetchBackendVersion } from '../../api/version/versionService';
import { footerContactService } from '../../api/common/footerContactService';
import type { FooterContact } from '../../types/admin/footerContactTypes';
import FooterLinkList from './FooterLinkList';
import mahlLogo from '../../assets/logos/Mahl_primary_V3.svg';
import xamkLogo from '../../assets/logos/xamk-logo.png';
import './Footer.scss';

const SOURCE_REPO_URL = 'https://github.com/xamkfi/myleague-app';
const XAMK_PROGRAMME_URL = 'https://www.xamk.fi/koulutukset/insinoori-amk-ohjelmistotekniikka/';

function GitHubIcon() {
  return (
    <svg
      className="footer-credits__github"
      viewBox="0 0 16 16"
      aria-hidden="true"
      focusable="false"
    >
      <path
        fill="currentColor"
        d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27s1.36.09 2 .27c1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0 0 16 8c0-4.42-3.58-8-8-8z"
      />
    </svg>
  );
}

export default function Footer() {
  const { t } = useTranslation();
  const [backendVersion, setBackendVersion] = useState<string>('...');
  const [entries, setEntries] = useState<FooterContact[]>([]);

  useEffect(() => {
    fetchBackendVersion().then(setBackendVersion);
    footerContactService
      .getAll()
      .then(setEntries)
      .catch(() => setEntries([]));
  }, []);

  const sports = entries.filter((item) => item.section === 'SeasonalSports');
  const activities = entries.filter((item) => item.section === 'OtherActivities');
  const contacts = entries.filter((item) => item.section === 'Contact' || !item.section);

  return (
    <footer className="footer">
      <img src={mahlLogo} alt="MAHL" className="footer-logo" />
      <div className="footer-sections">
        <div className="footer-section">
          <h4 className="footer-title">{t('footer.seasonSports', 'KAUSILAJIT')}</h4>
          <FooterLinkList
            items={sports}
            emptyLabel={t('footer.sports.empty', 'Kausilajeja ei ole vielä lisätty.')}
          />
        </div>
        <div className="footer-section">
          <h4 className="footer-title">{t('footer.otherActivities', 'MUU TOIMINTA')}</h4>
          <FooterLinkList
            items={activities}
            emptyLabel={t('footer.activities.empty', 'Muuta toimintaa ei ole vielä lisätty.')}
          />
        </div>
        <div className="footer-section">
          <h4 className="footer-title">{t('footer.contacts.title', 'YHTEYSTIEDOT')}</h4>
          <div className="footer-contact">
            {contacts.length === 0 ? (
              <p className="footer-contact-empty">
                {t('footer.contacts.empty', 'Yhteystietoja ei ole vielä lisätty.')}
              </p>
            ) : (
              contacts.map((contact) => (
                <div key={contact.id} className="footer-contact-entry">
                  <span>{contact.title}</span>
                  {contact.details && (
                    <span className="footer-contact-entry__details">{contact.details}</span>
                  )}
                  {contact.email && (
                    <a href={`mailto:${contact.email}`}>{contact.email}</a>
                  )}
                  {contact.phone && (
                    <a href={`tel:${contact.phone.replace(/\s+/g, '')}`}>{contact.phone}</a>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      </div>
      <div className="footer-version">
        <div className="footer-credits">
          <span className="footer-credits__made-by">
            {t('footer.credits.madeBy', 'Sivuston tehnyt')}
          </span>
          <a
            className="footer-credits__logo-link"
            href={XAMK_PROGRAMME_URL}
            target="_blank"
            rel="noopener noreferrer"
          >
            <img
              src={xamkLogo}
              alt={t('footer.credits.xamkLogo', 'XAMK, insinööri (AMK), ohjelmistotekniikka')}
              className="footer-credits__logo"
            />
          </a>
          <span className="footer-credits__programme">
            {t('footer.credits.programme', 'Ohjelmistotekniikka')}
          </span>
          <a
            className="footer-credits__source"
            href={SOURCE_REPO_URL}
            target="_blank"
            rel="noopener noreferrer"
          >
            <GitHubIcon />
            <span>{t('footer.credits.source', 'Lähdekoodit täällä')}</span>
          </a>
        </div>
        <Link className="footer-privacy-link" to="/tietosuojaseloste">
          {t('footer.privacyPolicy', 'Tietosuojaseloste')}
        </Link>
        <p className="footer-version__meta">
          Frontend: {__APP_VERSION__} | Backend: {backendVersion}
        </p>
      </div>
    </footer>
  );
}
