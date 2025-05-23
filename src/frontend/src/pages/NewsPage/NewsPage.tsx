import React from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate';
import './NewsPage.css';

function NewsPage() {
  const { t } = useTranslation();
  
  return (
    <PageTemplate title={t('nav.news')}>
      <div className="news-container">
        <article className="news-item">
          <h2>Lorem ipsum dolor sit amet</h2>
          <p className="news-date">05.05.2023</p>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies aliquam, nunc nisl aliquet nunc, quis aliquam nisl nunc quis nisl.</p>
        </article>
        <article className="news-item">
          <h2>Consectetur adipiscing elit</h2>
          <p className="news-date">01.05.2023</p>
          <p>Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante.</p>
        </article>
      </div>
    </PageTemplate>
  );
}

export default NewsPage; 