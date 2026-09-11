import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { fetchBackendVersion } from '../../api/version/versionService';
import { footerContactService } from '../../api/common/footerContactService';
import type { FooterContact } from '../../types/admin/footerContactTypes';
import FooterLinkList from './FooterLinkList';
import mahlLogo from '../../assets/logos/Mahl_primary_V3.svg';
import './Footer.scss';

function obfuscateEmail(email: string): string {
  return email.replace('@', ' (at) ');
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
                  {contact.email && <span>{obfuscateEmail(contact.email)}</span>}
                  {contact.phone && <span>{contact.phone}</span>}
                </div>
              ))
            )}
          </div>
        </div>
      </div>
      <div className="footer-version">
        Frontend: {__APP_VERSION__} | Backend: {backendVersion}
      </div>
    </footer>
  );
}
