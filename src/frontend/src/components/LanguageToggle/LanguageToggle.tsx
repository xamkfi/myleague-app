import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import './LanguageToggle.css';

const LanguageToggle: React.FC = () => {
  const { t, i18n } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  
  const currentLanguage = i18n.language;
  
  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
    setIsOpen(false);
  };
  
  return (
    <div className="language-toggle">
      <button 
        className="language-toggle-button"
        onClick={() => setIsOpen(!isOpen)}
        aria-expanded={isOpen}
      >
        {currentLanguage === 'fi' ? 'FI' : 'EN'}
        <span className="dropdown-icon">▼</span>
      </button>
      
      {isOpen && (
        <div className="language-dropdown">
          <button 
            className={`language-option ${currentLanguage === 'en' ? 'active' : ''}`}
            onClick={() => changeLanguage('en')}
          >
            {t('language.en')}
          </button>
          <button 
            className={`language-option ${currentLanguage === 'fi' ? 'active' : ''}`}
            onClick={() => changeLanguage('fi')}
          >
            {t('language.fi')}
          </button>
        </div>
      )}
    </div>
  );
};

export default LanguageToggle; 