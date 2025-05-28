import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import enTranslation from './locales/en/translation.json';
import fiTranslation from './locales/fi/translation.json';

// the translations
const resources = {
  en: {
    translation: enTranslation
  },
  fi: {
    translation: fiTranslation
  }
};

const isDevelopment = () => {
  return import.meta.env.MODE === 'development';
};

i18n
  // detect user language
  .use(LanguageDetector)
  // pass the i18n instance to react-i18next
  .use(initReactI18next)
  // init i18next
  .init({
    resources,
    fallbackLng: 'en',
    debug: isDevelopment(),
    
    interpolation: {
      escapeValue: false // not needed for react as it escapes by default
    },
    
    // language detection options
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    }
  });

export default i18n; 