import { useEffect, useId, useRef, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import './LanguageToggle.css';

function FinlandFlag(): ReactElement {
  return (
    <svg className="language-toggle-flag" viewBox="0 0 18 11" aria-hidden="true" focusable="false">
      <rect width="18" height="11" fill="#ffffff" />
      <rect width="18" height="3" y="4" fill="#003580" />
      <rect width="3" height="11" x="5" fill="#003580" />
    </svg>
  );
}

function UnitedKingdomFlag(): ReactElement {
  const clipId = `uk-flag-${useId().replace(/:/g, '')}`;

  return (
    <svg className="language-toggle-flag" viewBox="0 0 60 30" aria-hidden="true" focusable="false">
      <defs>
        <clipPath id={clipId}>
          <path d="M30,15 h30 v15 z v-15 h-30 z h-30 v-15 z v15 h30 z" />
        </clipPath>
      </defs>
      <rect width="60" height="30" fill="#012169" />
      <path d="M0,0 L60,30 M60,0 L0,30" stroke="#ffffff" strokeWidth="6" />
      <path d="M0,0 L60,30 M60,0 L0,30" stroke="#C8102E" strokeWidth="4" clipPath={`url(#${clipId})`} />
      <path d="M30,0 V30 M0,15 H60" stroke="#ffffff" strokeWidth="10" />
      <path d="M30,0 V30 M0,15 H60" stroke="#C8102E" strokeWidth="6" />
    </svg>
  );
}

const LANGUAGES = [
  { code: 'fi', Flag: FinlandFlag, labelKey: 'language.fi' },
  { code: 'en', Flag: UnitedKingdomFlag, labelKey: 'language.en' },
] as const;

type LanguageCode = (typeof LANGUAGES)[number]['code'];

function resolveLanguage(language: string): LanguageCode {
  return language.toLowerCase().startsWith('en') ? 'en' : 'fi';
}

function LanguageToggle() {
  const { t, i18n } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const currentLanguage = resolveLanguage(i18n.language);
  const current = LANGUAGES.find((language) => language.code === currentLanguage) ?? LANGUAGES[0];

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  const changeLanguage = (code: LanguageCode) => {
    void i18n.changeLanguage(code);
    setIsOpen(false);
  };

  return (
    <div ref={containerRef} className={`language-toggle${isOpen ? ' open' : ''}`}>
      <button
        type="button"
        className="language-toggle-button"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="listbox"
        aria-expanded={isOpen}
        aria-label={t('language.switchLanguage')}
        title={t(current.labelKey)}
      >
        <current.Flag />
        <svg
          className="language-toggle-caret"
          viewBox="0 0 12 8"
          width="12"
          height="8"
          aria-hidden="true"
          focusable="false"
        >
          <path d="M1 1.5 6 6.5l5-5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </button>

      {isOpen && (
        <ul className="language-dropdown" role="listbox" aria-label={t('language.switchLanguage')}>
          {LANGUAGES.map((language) => {
            const isSelected = language.code === currentLanguage;
            return (
              <li key={language.code} role="none">
                <button
                  type="button"
                  role="option"
                  aria-selected={isSelected}
                  className={`language-option${isSelected ? ' active' : ''}`}
                  onClick={() => changeLanguage(language.code)}
                >
                  <language.Flag />
                  <span>{t(language.labelKey)}</span>
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}

export default LanguageToggle;
