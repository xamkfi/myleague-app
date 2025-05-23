import React from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './MAHLPage.scss';

function MAHLPage() {
  const { t } = useTranslation();
  
  return (
    <PageTemplate title={t('nav.mahl')}>
      <div className="mahl-container">
        <section className="mahl-section">
          <h2>About MAHL</h2>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies aliquam, nunc nisl aliquet nunc, quis aliquam nisl nunc quis nisl.</p>
          
          <h3>Our Mission</h3>
          <p>Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante.</p>
        </section>
        
        <section className="mahl-section">
          <h2>Leadership</h2>
          <p>Donec eu libero sit amet quam egestas semper. Aenean ultricies mi vitae est. Mauris placerat eleifend leo.</p>
        </section>
      </div>
    </PageTemplate>
  );
}

export default MAHLPage; 