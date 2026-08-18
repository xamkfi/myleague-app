import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { AUDIENCE_REGISTRY, type AudienceThemeId } from '../../audience/audienceRegistry';
import { useAudience } from '../../context/AudienceContext';
import './AudienceSwitcher.scss';

interface AudienceSwitcherProps {
  /** 'block' stretches the control to full width for the mobile menu. */
  variant?: 'brand' | 'block';
}

function AudienceSwitcher({ variant = 'brand' }: AudienceSwitcherProps) {
  const { t } = useTranslation();
  const { audience, selectedAudienceId, setAudience } = useAudience();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isOpen) return;

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

  const handleSelect = (id: AudienceThemeId) => {
    setAudience(id);
    setIsOpen(false);
  };

  return (
    <div
      ref={containerRef}
      className={`audience-switcher audience-switcher--${variant}${isOpen ? ' open' : ''}`}
    >
      <button
        type="button"
        className="audience-switcher__trigger"
        onClick={() => setIsOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={isOpen}
        aria-label={t('audience.switcherLabel')}
      >
        <span className="audience-switcher__dot" aria-hidden="true" />
        <span className="audience-switcher__value">{t(audience.i18nKey)}</span>
        <svg
          className="audience-switcher__caret"
          viewBox="0 0 12 8"
          width="10"
          height="7"
          aria-hidden="true"
          focusable="false"
        >
          <path d="M1 1.5 6 6.5l5-5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </button>

      {isOpen && (
        <ul className="audience-switcher__menu" role="listbox" aria-label={t('audience.switcherLabel')}>
          {AUDIENCE_REGISTRY.map((entry) => {
            const isSelected = selectedAudienceId === entry.id;

            return (
              <li key={entry.id} role="none">
                <button
                  type="button"
                  role="option"
                  aria-selected={isSelected}
                  className={`audience-switcher__option${isSelected ? ' selected' : ''}`}
                  onClick={() => handleSelect(entry.id)}
                >
                  <span
                    className="audience-switcher__dot audience-switcher__dot--option"
                    data-audience-dot={entry.themeId}
                    aria-hidden="true"
                  />
                  {t(entry.i18nKey)}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}

export default AudienceSwitcher;
