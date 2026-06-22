import { useEffect, useState } from 'react';
import { fetchBackendVersion } from '../../api/version/versionService';
import { siteSettingsService, type FooterContactSettings } from '../../api/common/siteSettingsService';
import './Footer.scss';

export default function Footer() {
  const [backendVersion, setBackendVersion] = useState<string>('...');
  const [contactSettings, setContactSettings] = useState<FooterContactSettings | null>(null);

  useEffect(() => {
    fetchBackendVersion().then(setBackendVersion);
    siteSettingsService.getFooterContact()
      .then(setContactSettings)
      .catch((error) => {
        console.error('Failed to fetch footer contact settings:', error);
        setContactSettings(null);
      });
  }, []);

  const hasOrganization = Boolean(contactSettings?.organizationName?.trim() || contactSettings?.organizationAddress?.trim());
  const contactPersons = contactSettings?.contactPersons?.filter(
    (person) => person.nameOrRole?.trim() || person.email?.trim() || person.phone?.trim(),
  ) ?? [];

  return (
    <footer className="footer">
      <div className="footer-sections">
        <div className="footer-section">
          <h4 className="footer-title">KAUSILAJIT</h4>
          <div className="footer-links">
            <span>Jalkapallo</span>
            <span>Jääkiekko</span>
            <span>Salibandy</span>
            <span>Salibandyn Manager</span>
            <span>Talvijalkapallo</span>
            <span>Jääpallo</span>
            <span>Puumalaliga</span>
            <span>Jääkiekko +40</span>
          </div>
        </div>
        <div className="footer-section">
          <h4 className="footer-title">MUU TOIMINTA</h4>
          <div className="footer-links">
            <span>PMT Turnaukset</span>
            <span>Korttelitoiminta</span>
            <span>WHL Liikuntaleirit</span>
            <span>Turnauspiste</span>
          </div>
        </div>
        <div className="footer-section">
          <h4 className="footer-title">YHTEYSTIEDOT</h4>
          <div className="footer-contact">
            {hasOrganization && (
              <div>
                {contactSettings?.organizationName && <>{contactSettings.organizationName}<br /></>}
                {contactSettings?.organizationAddress}
              </div>
            )}

            {contactPersons.map((person, index) => (
              <div key={`${person.nameOrRole}-${person.email}-${index}`}>
                {person.nameOrRole && <>{person.nameOrRole}<br /></>}
                {person.email && <>{person.email}<br /></>}
                {person.phone}
              </div>
            ))}

            {!hasOrganization && contactPersons.length === 0 && (
              <div>Yhteystietoja ei saatavilla.</div>
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
