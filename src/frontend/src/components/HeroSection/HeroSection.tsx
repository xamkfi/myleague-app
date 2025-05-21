import React from 'react';
import { useTranslation } from 'react-i18next';
import './HeroSection.css';

interface HeroSectionProps {
  title?: string;
  buttonText?: string;
  onButtonClick?: () => void;
}

const HeroSection: React.FC<HeroSectionProps> = ({ 
  title,
  buttonText,
  onButtonClick 
}) => {
  const { t } = useTranslation();
  
  return (
    <div className="hero-section">
      <div className="hero-content">
        <h1 className="hero-title">{title || t('hero.title')}</h1>
        <button className="hero-button" onClick={onButtonClick}>
          {buttonText || t('hero.buttonText')}
        </button>
      </div>
    </div>
  );
};

export default HeroSection; 