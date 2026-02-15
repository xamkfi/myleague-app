import { useEffect, useState } from 'react';
import { fetchBackendVersion } from '../../api/version/versionService';
import './Footer.scss';

export default function Footer() {
  const [backendVersion, setBackendVersion] = useState<string>('...');

  useEffect(() => {
    fetchBackendVersion().then(setBackendVersion);
  }, []);

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
            <div>Mikkelin alueen harrasteliigat ry<br/>Savilahdenkatu 12 B 23<br/>50100 MIKKELI</div>
            <div>Seuratyöntekijä Pasi (asukasmiehet)<br/>pasi (at) mahl.fi<br/>044 209 9919</div>
            <div>Seuratyöntekijä Mikko Loukonen<br/>mikko (at) mahl.fi<br/>044 209 9919</div>
          </div>
        </div>
      </div>
      <div className="footer-version">
        Frontend: {__APP_VERSION__} | Backend: {backendVersion}
      </div>
    </footer>
  );
}
