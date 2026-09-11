import { useEffect, useMemo, useRef, useState, type KeyboardEvent } from 'react';
import { useTranslation } from 'react-i18next';

interface GoalieOption {
  playerId: string;
  playerName: string;
  jerseyNumber: number | null;
}

interface GoalieSearchSelectProps {
  players: GoalieOption[];
  value: string;
  onChange: (playerId: string) => void;
  disabled?: boolean;
}

function playerLabel(player: GoalieOption): string {
  return player.jerseyNumber != null
    ? `#${player.jerseyNumber} ${player.playerName}`
    : player.playerName;
}

function matchesQuery(player: GoalieOption, query: string): boolean {
  const needle = query.trim().toLowerCase();
  if (!needle) {
    return true;
  }
  const jersey = player.jerseyNumber != null ? String(player.jerseyNumber) : '';
  return `${player.playerName} ${jersey} #${jersey}`.toLowerCase().includes(needle);
}

export default function GoalieSearchSelect({
  players,
  value,
  onChange,
  disabled = false,
}: GoalieSearchSelectProps) {
  const { t } = useTranslation();
  const rootRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [highlightedIndex, setHighlightedIndex] = useState(0);

  const selected = useMemo(
    () => players.find((player) => player.playerId === value) ?? null,
    [players, value],
  );

  const filtered = useMemo(
    () => players.filter((player) => matchesQuery(player, query)),
    [players, query],
  );

  useEffect(() => {
    if (!isOpen) {
      return;
    }
    setHighlightedIndex(0);
  }, [query, isOpen]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        setQuery('');
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selectPlayer = (playerId: string) => {
    onChange(playerId);
    setIsOpen(false);
    setQuery('');
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      setIsOpen(true);
      setHighlightedIndex((index) => Math.min(index + 1, Math.max(filtered.length - 1, 0)));
      return;
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      setHighlightedIndex((index) => Math.max(index - 1, 0));
      return;
    }
    if (event.key === 'Enter' && isOpen) {
      event.preventDefault();
      const match = filtered[highlightedIndex];
      if (match) {
        selectPlayer(match.playerId);
      }
      return;
    }
    if (event.key === 'Escape') {
      setIsOpen(false);
      setQuery('');
    }
  };

  return (
    <div
      ref={rootRef}
      className={`club-admin-goalie-search${disabled ? ' club-admin-goalie-search--disabled' : ''}`}
    >
      <input
        ref={inputRef}
        id="goalie-select"
        type="search"
        className="club-admin-goalie-search__input"
        value={isOpen ? query : selected ? playerLabel(selected) : ''}
        placeholder={t('clubAdmin.searchGoalie')}
        disabled={disabled}
        autoComplete="off"
        aria-autocomplete="list"
        aria-expanded={isOpen}
        aria-controls="goalie-search-list"
        role="combobox"
        onFocus={() => {
          setIsOpen(true);
          setQuery('');
        }}
        onChange={(event) => {
          setQuery(event.target.value);
          setIsOpen(true);
        }}
        onKeyDown={handleKeyDown}
      />
      {value && !disabled && (
        <button
          type="button"
          className="club-admin-goalie-search__clear"
          onClick={() => selectPlayer('')}
        >
          {t('clubAdmin.clearGoalie')}
        </button>
      )}
      {isOpen && !disabled && (
        <ul id="goalie-search-list" className="club-admin-goalie-search__list" role="listbox">
          <li>
            <button
              type="button"
              className={`club-admin-goalie-search__option${value === '' ? ' club-admin-goalie-search__option--active' : ''}`}
              role="option"
              aria-selected={value === ''}
              onMouseEnter={() => setHighlightedIndex(-1)}
              onClick={() => selectPlayer('')}
            >
              {t('clubAdmin.noGoalie')}
            </button>
          </li>
          {filtered.length === 0 ? (
            <li className="club-admin-goalie-search__empty">{t('clubAdmin.noMatchingGoalies')}</li>
          ) : (
            filtered.map((player, index) => (
              <li key={player.playerId}>
                <button
                  type="button"
                  className={[
                    'club-admin-goalie-search__option',
                    player.playerId === value ? 'club-admin-goalie-search__option--selected' : '',
                    index === highlightedIndex ? 'club-admin-goalie-search__option--active' : '',
                  ].filter(Boolean).join(' ')}
                  role="option"
                  aria-selected={player.playerId === value}
                  onMouseEnter={() => setHighlightedIndex(index)}
                  onClick={() => selectPlayer(player.playerId)}
                >
                  {player.jerseyNumber != null && (
                    <span className="club-admin-jersey-badge">#{player.jerseyNumber}</span>
                  )}
                  <span>{player.playerName}</span>
                </button>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  );
}
