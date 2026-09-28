import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import './SeasonStandingsSettings.scss';

interface SeasonStandingsSettingsProps {
  idPrefix: string;
  teamsAdvancing: number;
  criteria: string[];
  options: readonly string[];
  allowAddRemove: boolean;
  disabled?: boolean;
  onTeamsAdvancingChange: (value: number) => void;
  onCriteriaChange: (criteria: string[]) => void;
}

export default function SeasonStandingsSettings({
  idPrefix,
  teamsAdvancing,
  criteria,
  options,
  allowAddRemove,
  disabled = false,
  onTeamsAdvancingChange,
  onCriteriaChange,
}: SeasonStandingsSettingsProps) {
  const { t } = useTranslation();
  const remaining = options.filter((option) => !criteria.includes(option));
  const [pending, setPending] = useState(remaining[0] ?? '');

  const move = (index: number, direction: -1 | 1): void => {
    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= criteria.length) {
      return;
    }
    const next = [...criteria];
    const [item] = next.splice(index, 1);
    next.splice(nextIndex, 0, item);
    onCriteriaChange(next);
  };

  const remove = (index: number): void => {
    if (criteria.length <= 1) {
      return;
    }
    onCriteriaChange(criteria.filter((_, itemIndex) => itemIndex !== index));
  };

  const add = (): void => {
    const value = pending || remaining[0];
    if (!value || criteria.includes(value)) {
      return;
    }
    onCriteriaChange([...criteria, value]);
    const nextRemaining = remaining.filter((option) => option !== value);
    setPending(nextRemaining[0] ?? '');
  };

  return (
    <div className="form-section season-standings-settings">
      <h3 className="form-section__title">{t('seasonStandings.title')}</h3>
      <div className="form-group">
        <label htmlFor={`${idPrefix}-teams-advancing`}>{t('seasonStandings.teamsAdvancing')}</label>
        <input
          id={`${idPrefix}-teams-advancing`}
          type="number"
          min={0}
          value={teamsAdvancing}
          disabled={disabled}
          onChange={(event) => onTeamsAdvancingChange(Math.max(0, Number.parseInt(event.target.value, 10) || 0))}
        />
        <p className="season-standings-settings__hint">{t('seasonStandings.teamsAdvancingHint')}</p>
      </div>
      <div className="form-group">
        <span className="season-standings-settings__label">{t('seasonStandings.ranking')}</span>
        <p className="season-standings-settings__hint">{t('seasonStandings.rankingHint')}</p>
        <ol className="season-standings-settings__list">
          {criteria.map((item, index) => (
            <li key={item} className="season-standings-settings__item">
              <span>{t(`seasonStandings.criteria.${item}`, item)}</span>
              <span className="season-standings-settings__actions">
                <button type="button" disabled={disabled || index === 0} onClick={() => move(index, -1)} aria-label={t('seasonStandings.moveUp')}>
                  ↑
                </button>
                <button type="button" disabled={disabled || index === criteria.length - 1} onClick={() => move(index, 1)} aria-label={t('seasonStandings.moveDown')}>
                  ↓
                </button>
                {allowAddRemove && (
                  <button type="button" disabled={disabled || criteria.length <= 1} onClick={() => remove(index)}>
                    {t('seasonStandings.remove')}
                  </button>
                )}
              </span>
            </li>
          ))}
        </ol>
        {allowAddRemove && remaining.length > 0 && (
          <div className="season-standings-settings__add">
            <select
              aria-label={t('seasonStandings.add')}
              value={remaining.includes(pending) ? pending : remaining[0]}
              disabled={disabled}
              onChange={(event) => setPending(event.target.value)}
            >
              {remaining.map((option) => (
                <option key={option} value={option}>
                  {t(`seasonStandings.criteria.${option}`, option)}
                </option>
              ))}
            </select>
            <button type="button" disabled={disabled} onClick={add}>
              {t('seasonStandings.add')}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
