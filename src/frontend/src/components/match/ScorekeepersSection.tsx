import { useCallback, useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { personApi } from '../../api/admin/personApi';
import type { Person } from '../../types/admin/personTypes';
import type { MatchPersonDto } from '../../types/common/matchPersonTypes';
import './ScorekeepersSection.scss';

const SEARCH_PAGE_SIZE = 25;
const SEARCH_DEBOUNCE_MS = 300;
const MIN_SEARCH_LENGTH = 2;

interface ScorekeepersSectionProps {
  scorekeepers: MatchPersonDto[];
  saving: boolean;
  onAdd: (personId: string) => Promise<void>;
  onRemove: (personId: string) => Promise<void>;
  /** Read-only mode for completed matches: hides the add and remove actions. */
  disabled?: boolean;
}

/**
 * Manage-match card for the optional scorekeepers (toimitsijat). Persons are picked from the
 * person registry through a search modal.
 */
function ScorekeepersSection({ scorekeepers, saving, onAdd, onRemove, disabled = false }: ScorekeepersSectionProps) {
  const { t } = useTranslation();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);

  return (
    <section
      className="scorekeepers-section"
      aria-label={t('matchScorekeepers.title', 'Match scorekeepers')}
    >
      <div className="scorekeepers-section__header">
        <h3 className="scorekeepers-section__title">{t('matchScorekeepers.title', 'Match scorekeepers')}</h3>
        {!disabled && (
          <button
            type="button"
            className="scorekeepers-section__add"
            onClick={() => setIsModalOpen(true)}
            disabled={saving}
          >
            <i className="fas fa-plus" aria-hidden="true"></i>
            {t('matchScorekeepers.add', 'Add scorekeeper')}
          </button>
        )}
      </div>

      {scorekeepers.length === 0 ? (
        <div className="scorekeepers-section__empty">
          {t('matchScorekeepers.none', 'No scorekeepers assigned (optional).')}
        </div>
      ) : (
        <ul className="scorekeepers-section__rows">
          {scorekeepers.map((scorekeeper) => (
            <li className="scorekeepers-section__row" key={scorekeeper.id}>
              <span className="scorekeepers-section__name">
                {scorekeeper.name || t('matchScorekeepers.unknownPerson', 'Unknown person')}
              </span>
              {!disabled && (
                <button
                  type="button"
                  className="scorekeepers-section__remove"
                  onClick={() => void onRemove(scorekeeper.id)}
                  disabled={saving}
                  aria-label={t('matchScorekeepers.remove', 'Remove scorekeeper')}
                >
                  ×
                </button>
              )}
            </li>
          ))}
        </ul>
      )}

      {isModalOpen && (
        <ScorekeeperSearchModal
          selectedIds={scorekeepers.map((s) => s.id)}
          saving={saving}
          onAdd={onAdd}
          onClose={() => setIsModalOpen(false)}
        />
      )}
    </section>
  );
}

interface ScorekeeperSearchModalProps {
  selectedIds: string[];
  saving: boolean;
  onAdd: (personId: string) => Promise<void>;
  onClose: () => void;
}

function ScorekeeperSearchModal({ selectedIds, saving, onAdd, onClose }: ScorekeeperSearchModalProps) {
  const { t } = useTranslation();
  const [search, setSearch] = useState<string>('');
  const [persons, setPersons] = useState<Person[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const debounceTimerRef = useRef<number | null>(null);
  const requestIdRef = useRef<number>(0);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const fetchPersons = useCallback(async (term: string): Promise<void> => {
    const requestId: number = ++requestIdRef.current;
    setError(null);
    const trimmed: string = term.trim();
    if (trimmed.length < MIN_SEARCH_LENGTH) {
      setPersons([]);
      setIsSearching(false);
      return;
    }
    setIsSearching(true);
    try {
      const response = await personApi.search(trimmed, 1, SEARCH_PAGE_SIZE);
      if (requestId !== requestIdRef.current) return;
      setPersons(response.data ?? []);
    } catch (err: unknown) {
      if (requestId !== requestIdRef.current) return;
      setPersons([]);
      setError(err instanceof Error ? err.message : t('matchScorekeepers.searchError', 'Failed to search persons'));
    } finally {
      if (requestId === requestIdRef.current) setIsSearching(false);
    }
  }, [t]);

  useEffect(() => {
    searchInputRef.current?.focus();
    return () => {
      if (debounceTimerRef.current !== null) clearTimeout(debounceTimerRef.current);
    };
  }, []);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  const handleSearchChange = (value: string): void => {
    setSearch(value);
    if (value.trim().length >= MIN_SEARCH_LENGTH) setIsSearching(true);
    if (debounceTimerRef.current !== null) clearTimeout(debounceTimerRef.current);
    debounceTimerRef.current = window.setTimeout(() => {
      void fetchPersons(value);
    }, SEARCH_DEBOUNCE_MS);
  };

  return (
    <div className="scorekeepers-modal__overlay" onClick={onClose} role="presentation">
      <div
        className="scorekeepers-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="scorekeepers-modal-title"
      >
        <header className="scorekeepers-modal__header">
          <h3 id="scorekeepers-modal-title">{t('matchScorekeepers.modalTitle', 'Select scorekeeper')}</h3>
          <button
            type="button"
            className="scorekeepers-modal__close"
            onClick={onClose}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </header>

        <input
          ref={searchInputRef}
          type="search"
          className="scorekeepers-modal__search"
          placeholder={t('matchScorekeepers.searchPlaceholder', 'Search persons by name...')}
          value={search}
          onChange={(event) => handleSearchChange(event.target.value)}
        />

        {error && <div className="scorekeepers-modal__error">{error}</div>}
        {isSearching && <div className="scorekeepers-modal__status">{t('common.loading', 'Loading...')}</div>}

        {!isSearching && !error && search.trim().length < MIN_SEARCH_LENGTH && (
          <div className="scorekeepers-modal__status">
            {t('matchScorekeepers.searchHint', 'Type at least 2 characters to search all persons.')}
          </div>
        )}

        {!isSearching && !error && search.trim().length >= MIN_SEARCH_LENGTH && (
          <ul className="scorekeepers-modal__results">
            {persons.length === 0 ? (
              <li className="scorekeepers-modal__status">{t('matchScorekeepers.noPersons', 'No persons found.')}</li>
            ) : (
              persons.map((person) => {
                const alreadyAdded: boolean = selectedIds.includes(person.id);
                return (
                  <li key={person.id} className="scorekeepers-modal__result">
                    <span>{person.fullName || `${person.firstName} ${person.lastName}`}</span>
                    <button
                      type="button"
                      className="scorekeepers-modal__pick"
                      disabled={alreadyAdded || saving}
                      onClick={() => void onAdd(person.id)}
                    >
                      {alreadyAdded
                        ? t('matchScorekeepers.added', 'Added')
                        : t('matchScorekeepers.pick', 'Add')}
                    </button>
                  </li>
                );
              })
            )}
          </ul>
        )}
      </div>
    </div>
  );
}

export default ScorekeepersSection;
