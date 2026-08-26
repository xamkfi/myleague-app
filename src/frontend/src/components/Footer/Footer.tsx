import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { fetchBackendVersion } from '../../api/version/versionService';
import { footerContactService } from '../../api/common/footerContactService';
import type { FooterContact } from '../../types/admin/footerContactTypes';
import './Footer.scss';

function obfuscateEmail(email: string): string {
  return email.replace('@', ' (at) ');
}

export default function Footer() {
  const { t } = useTranslation();
  const [backendVersion, setBackendVersion] = useState<string>('...');
  const [contacts, setContacts] = useState<FooterContact[]>([]);

  useEffect(() => {
    fetchBackendVersion().then(setBackendVersion);
    footerContactService
      .getAll()
      .then(setContacts)
      .catch(() => setContacts([]));
  }, []);

  return (
    <footer className="footer">
      <div className="footer-sections">
        <div className="footer-section">
          <h4 className="footer-title">{t('footer.seasonSports', 'KAUSILAJIT')}</h4>
          <div className="footer-links">
            <span>{t('footer.sports.football', 'Jalkapallo')}</span>
            <span>{t('footer.sports.iceHockey', 'Jääkiekko')}</span>
            <span>{t('footer.sports.floorball', 'Salibandy')}</span>
            <span>{t('footer.sports.floorballManager', 'Salibandyn Manager')}</span>
            <span>{t('footer.sports.winterFootball', 'Talvijalkapallo')}</span>
            <span>{t('footer.sports.bandy', 'Jääpallo')}</span>
            <span>{t('footer.sports.puumalaliga', 'Puumalaliga')}</span>
            <span>{t('footer.sports.iceHockey40', 'Jääkiekko +40')}</span>
          </div>
        </div>
        <div className="footer-section">
          <h4 className="footer-title">{t('footer.otherActivities', 'MUU TOIMINTA')}</h4>
          <div className="footer-links">
            <span>{t('footer.activities.pmt', 'PMT Turnaukset')}</span>
            <span>{t('footer.activities.kortteli', 'Korttelitoiminta')}</span>
            <span>{t('footer.activities.whl', 'WHL Liikuntaleirit')}</span>
            <span>{t('footer.activities.turnauspiste', 'Turnauspiste')}</span>
          </div>
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
                <article key={contact.id} className="footer-contact-card">
                  <h5 className="footer-contact-card__title">{contact.title}</h5>
                  {contact.details && (
                    <p className="footer-contact-card__details">{contact.details}</p>
                  )}
                  {contact.email && <p>{obfuscateEmail(contact.email)}</p>}
                  {contact.phone && <p>{contact.phone}</p>}
                  {contact.url && (
                    <a
                      className="footer-contact-card__link"
                      href={contact.url}
                      target="_blank"
                      rel="noreferrer noopener"
                    >
                      {contact.url.replace(/^https?:\/\//, '')}
                    </a>
                  )}
                </article>
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
