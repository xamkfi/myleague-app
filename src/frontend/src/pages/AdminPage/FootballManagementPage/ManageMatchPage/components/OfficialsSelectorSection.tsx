import type { ChangeEvent } from 'react';
import { useTranslation } from 'react-i18next';
import './OfficialsSelectorSection.scss';

export interface OfficialOption {
  id: string;
  name: string;
}

interface OfficialsSelectorSectionProps {
  selectedOfficials: string[];
  options: OfficialOption[];
  saving: boolean;
  onAddRow: () => void;
  onSelect: (index: number, refereeId: string) => void;
  onRemove: (index: number, refereeId: string) => void;
  /**
   * When true, the entire section becomes read-only: the Add referee action is hidden,
   * and existing rows cannot be changed or removed. Used to lock officials editing after
   * the match has been Completed — the operator must reopen the match first.
   */
  disabled?: boolean;
}

const OfficialsSelectorSection = ({
  selectedOfficials,
  options,
  saving,
  onAddRow,
  onSelect,
  onRemove,
  disabled = false,
}: OfficialsSelectorSectionProps) => {
  const { t } = useTranslation();
  const handleChange = (index: number, event: ChangeEvent<HTMLSelectElement>) => {
    onSelect(index, event.target.value);
  };

  const isLocked: boolean = disabled || saving;

  return (
    <section
      className="officials-selector-section"
      aria-label={t('football.matches.manage.matchOfficials', 'Match officials')}
    >
      <div className="officials-selector-section__header">
        <h3 className="officials-selector-section__title">
          {t('football.matches.manage.matchOfficials', 'MATCH OFFICIALS')}
        </h3>
        {/* Hide the Add affordance entirely when the section is locked (Completed match): */}
        {/* keeping a greyed-out button there would just invite frustrated clicks.        */}
        {!disabled && (
          <button
            type="button"
            className="officials-selector-section__add"
            onClick={onAddRow}
            disabled={saving}
          >
            <i className="fas fa-plus" aria-hidden="true"></i>
            {t('football.matches.manage.addReferee', 'Add referee')}
          </button>
        )}
      </div>

      {selectedOfficials.length === 0 ? (
        <div className="officials-selector-section__empty">
          {t('football.matches.manage.noOfficials', 'No officials assigned.')}
        </div>
      ) : (
        <div className="officials-selector-section__rows">
          {selectedOfficials.map((refId, idx) => (
            <div className="officials-selector-section__row" key={`${idx}-${refId || 'empty'}`}>
              <select
                value={refId}
                onChange={(e) => handleChange(idx, e)}
                disabled={isLocked}
              >
                <option value="">
                  {saving
                    ? t('football.matches.manage.saving', 'Saving...')
                    : t('football.matches.manage.selectReferee', 'SELECT REFEREE')}
                </option>
                {options.map(option => (
                  <option
                    key={option.id}
                    value={option.id}
                    disabled={selectedOfficials.includes(option.id) && option.id !== refId}
                  >
                    {option.name}
                  </option>
                ))}
              </select>
              {/* Match the lineup card: only show the destructive action when editing is allowed. */}
              {!disabled && (
                <button
                  type="button"
                  className="officials-selector-section__remove"
                  onClick={() => onRemove(idx, refId)}
                  disabled={isLocked || !refId}
                  aria-label={t('football.matches.manage.removeReferee', 'Remove referee')}
                >
                  ×
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  );
};

export default OfficialsSelectorSection;
