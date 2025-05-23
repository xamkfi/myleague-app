import React from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate';
import './RulesPage.scss';

function RulesPage() {
  const { t } = useTranslation();
  
  return (
    <PageTemplate title={t('nav.rules')}>
      <div className="rules-container">
        <section className="rule-section">
          <h2>General Rules</h2>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies aliquam, nunc nisl aliquet nunc, quis aliquam nisl nunc quis nisl.</p>
          
          <h3>Rule 1</h3>
          <p>Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante.</p>
          
          <h3>Rule 2</h3>
          <p>Donec eu libero sit amet quam egestas semper. Aenean ultricies mi vitae est. Mauris placerat eleifend leo.</p>
        </section>
        
        <section className="rule-section">
          <h2>Game Rules</h2>
          <p>Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.</p>
          
          <h3>Rule 3</h3>
          <p>Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante. Donec eu libero sit amet quam egestas semper.</p>
          
          <h3>Rule 4</h3>
          <p>Aenean ultricies mi vitae est. Mauris placerat eleifend leo. Quisque sit amet est et sapien ullamcorper pharetra.</p>
        </section>
      </div>
    </PageTemplate>
  );
}

export default RulesPage; 