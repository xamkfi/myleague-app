import React from 'react';
import { useTranslation } from 'react-i18next';
import './HeroSection.scss';

interface HeroSectionProps {
  title?: string;
  buttonText?: string;
  onButtonClick?: () => void;
}

function HeroSection({ 
  title,
  buttonText,
  onButtonClick 
}: HeroSectionProps) {
  const { t } = useTranslation();
  
  return (
    <div className="hero-section">
      <div className="content-container">
        <h1 className="title-xl">{title || t('hero.title')}</h1>
        <button className="button button-primary" onClick={onButtonClick}>
          {buttonText || t('hero.buttonText')}
        </button>
      </div>
    </div>
  );
}

export default HeroSection; 