import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { personApi } from '../../../api/admin/personApi';
import { userService } from '../../../api/admin/userService';
import type { Person } from '../../../types/admin/personTypes';
import type { SystemUser } from '../../../types/admin/userTypes';
import './ClubAdminsPicker.scss';

export interface ClubAdminSelection {
  /** Existing user account, or null when the person still needs to be invited on save. */
  userId: string | null;
  personId: string;
  firstName: string;
  lastName: string;
  email: string;
}

interface ClubAdminsPickerProps {
  selectedAdmins: ClubAdminSelection[];
  onChange: (admins: ClubAdminSelection[]) => void;
}

const SEARCH_PAGE_SIZE = 25;
const SEARCH_DEBOUNCE_MS = 300;

export function getClubAdminDisplayName(admin: ClubAdminSelection): string {
  const name = `${admin.firstName} ${admin.lastName}`.trim();
  return name || admin.email;
}

function personEmail(person: Person): string {
  return person.contactInfo?.email?.trim() ?? '';
}

/**
 * Site-admin control for assigning club admins: shows the current managers with a
 * remove action, and a button that opens a person-search modal to add more.
 */
function ClubAdminsPicker({ selectedAdmins, onChange }: ClubAdminsPickerProps) {
  const { t } = useTranslation();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const removeAdmin = (personId: string) => {
    onChange(selectedAdmins.filter((admin) => admin.personId !== personId));
  };

  return (
    <div className="club-admins-picker">
      <div className="club-admins-picker__header">
        <label className="club-admins-picker__label">
          {t('clubs.admins.title', 'Club admins')}
          {selectedAdmins.length > 0 && (
            <span className="club-admins-picker__count">{selectedAdmins.length}</span>
          )}
        </label>
        <p className="club-admins-picker__hint">
          {t(
            'clubs.admins.hint',
            'Club admins can edit the club information and manage rosters and match lineups for the teams under the club.',
          )}
        </p>
      </div>

      {selectedAdmins.length === 0 ? (
        <div className="club-admins-picker__empty">
          {t('clubs.admins.noneSelected', 'No club admins assigned yet.')}
        </div>
      ) : (
        <ul className="club-admins-picker__selected">
          {selectedAdmins.map((admin) => (
            <li key={admin.personId} className="club-admins-picker__selected-item">
              <div className="club-admins-picker__selected-info">
                <span className="club-admins-picker__name">{getClubAdminDisplayName(admin)}</span>
                {admin.email && (
                  <span className="club-admins-picker__email">{admin.email}</span>
                )}
              </div>
              <button
                type="button"
                className="club-admins-picker__remove"
                onClick={() => removeAdmin(admin.personId)}
              >
                {t('clubs.admins.remove', 'Remove')}
              </button>
            </li>
          ))}
        </ul>
      )}

      <button
        type="button"
        className="club-admins-picker__add-btn"
        onClick={() => setIsModalOpen(true)}
      >
        {t('clubs.admins.addButton', 'Add club admin')}
      </button>

      {isModalOpen && (
        <ClubAdminsModal
          selectedAdmins={selectedAdmins}
          onChange={onChange}
          onClose={() => setIsModalOpen(false)}
        />
      )}
    </div>
  );
}

interface ClubAdminsModalProps {
  selectedAdmins: ClubAdminSelection[];
  onChange: (admins: ClubAdminSelection[]) => void;
  onClose: () => void;
}

export function ClubAdminsModal({ selectedAdmins, onChange, onClose }: ClubAdminsModalProps) {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [persons, setPersons] = useState<Person[]>([]);
  const [usersByPersonId, setUsersByPersonId] = useState<Map<string, SystemUser>>(new Map());
  const [isSearching, setIsSearching] = useState(false);
  const [isLoadingUsers, setIsLoadingUsers] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [addError, setAddError] = useState<string | null>(null);
  const debounceTimerRef = useRef<number | null>(null);
  const searchAbortRef = useRef<AbortController | null>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const selectedPersonIds = useMemo(
    () => new Set(selectedAdmins.map((admin) => admin.personId)),
    [selectedAdmins],
  );

  useEffect(() => {
    document.body.style.overflow = 'hidden';
    searchInputRef.current?.focus();

    return () => {
      document.body.style.overflow = '';
      if (debounceTimerRef.current !== null) {
        clearTimeout(debounceTimerRef.current);
      }
      searchAbortRef.current?.abort();
    };
  }, []);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  useEffect(() => {
    let cancelled = false;

    const loadUsers = async () => {
      try {
        const users = await userService.getAll();
        if (cancelled) return;
        const map = new Map<string, SystemUser>();
        for (const user of users) {
          map.set(user.personId, user);
        }
        setUsersByPersonId(map);
      } catch (err: unknown) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : t('clubs.admins.loadError', 'Failed to load users'));
        }
      } finally {
        if (!cancelled) setIsLoadingUsers(false);
      }
    };

    void loadUsers();
    return () => {
      cancelled = true;
    };
  }, [t]);

  const fetchPersons = useCallback(async (term: string) => {
    searchAbortRef.current?.abort();
    const controller = new AbortController();
    searchAbortRef.current = controller;

    setIsSearching(true);
    setError(null);

    try {
      const trimmed = term.trim();
      const response = trimmed.length >= 2
        ? await personApi.search(trimmed, 1, SEARCH_PAGE_SIZE)
        : await personApi.getAll(1, SEARCH_PAGE_SIZE);

      if (controller.signal.aborted) return;
      setPersons(response.data ?? []);
    } catch (err: unknown) {
      if (controller.signal.aborted) return;
      setPersons([]);
      setError(err instanceof Error ? err.message : t('clubs.admins.searchError', 'Failed to search persons'));
    } finally {
      if (!controller.signal.aborted) {
        setIsSearching(false);
      }
    }
  }, [t]);

  useEffect(() => {
    void fetchPersons('');
  }, [fetchPersons]);

  const handleSearchChange = (value: string) => {
    setSearch(value);
    setAddError(null);

    if (debounceTimerRef.current !== null) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = window.setTimeout(() => {
      void fetchPersons(value);
    }, SEARCH_DEBOUNCE_MS);
  };

  const addPerson = (person: Person) => {
    setAddError(null);

    if (selectedPersonIds.has(person.id)) {
      return;
    }

    const existingUser = usersByPersonId.get(person.id);
    const email = existingUser?.email || personEmail(person);

    if (!email) {
      setAddError(t(
        'clubs.admins.emailRequired',
        'This person has no email address, so they cannot be invited as a club admin.',
      ));
      return;
    }

    onChange([
      ...selectedAdmins,
      {
        userId: existingUser?.id ?? null,
        personId: person.id,
        firstName: person.firstName,
        lastName: person.lastName,
        email,
      },
    ]);
  };

  const removeAdmin = (personId: string) => {
    onChange(selectedAdmins.filter((admin) => admin.personId !== personId));
  };

  return (
    <div className="club-admins-modal__overlay" onClick={onClose} role="presentation">
      <div
        className="club-admins-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="club-admins-modal-title"
      >
        <header className="club-admins-modal__header">
          <h3 id="club-admins-modal-title">
            {t('clubs.admins.modalTitle', 'Manage club admins')}
          </h3>
        </header>

        <section className="club-admins-modal__body">
          <div className="club-admins-modal__section">
            <h4>{t('clubs.admins.currentTitle', 'Current club admins')}</h4>
            {selectedAdmins.length === 0 ? (
              <p className="club-admins-picker__empty">
                {t('clubs.admins.noneSelected', 'No club admins assigned yet.')}
              </p>
            ) : (
              <ul className="club-admins-picker__selected">
                {selectedAdmins.map((admin) => (
                  <li key={admin.personId} className="club-admins-picker__selected-item">
                    <div className="club-admins-picker__selected-info">
                      <span className="club-admins-picker__name">{getClubAdminDisplayName(admin)}</span>
                      {admin.email && (
                        <span className="club-admins-picker__email">{admin.email}</span>
                      )}
                    </div>
                    <button
                      type="button"
                      className="club-admins-picker__remove"
                      onClick={() => removeAdmin(admin.personId)}
                    >
                      {t('clubs.admins.remove', 'Remove')}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="club-admins-modal__section">
            <h4>{t('clubs.admins.searchTitle', 'Search persons')}</h4>
            <input
              ref={searchInputRef}
              type="search"
              className="club-admins-modal__search"
              placeholder={t('clubs.admins.searchPlaceholder', 'Search persons by name...')}
              value={search}
              onChange={(event) => handleSearchChange(event.target.value)}
            />

            {addError && <div className="club-admins-picker__error">{addError}</div>}
            {error && <div className="club-admins-picker__error">{error}</div>}

            {(isSearching || isLoadingUsers) && (
              <div className="club-admins-picker__loading">{t('common.loading', 'Loading...')}</div>
            )}

            {!isSearching && !isLoadingUsers && !error && (
              <ul className="club-admins-modal__results">
                {persons.length === 0 ? (
                  <li className="club-admins-picker__empty">
                    {t('clubs.admins.noPersons', 'No persons found.')}
                  </li>
                ) : (
                  persons.map((person) => {
                    const alreadyAdded = selectedPersonIds.has(person.id);
                    const existingUser = usersByPersonId.get(person.id);
                    const email = existingUser?.email || personEmail(person);

                    return (
                      <li key={person.id} className="club-admins-modal__result">
                        <div className="club-admins-picker__selected-info">
                          <span className="club-admins-picker__name">{person.fullName}</span>
                          {email && <span className="club-admins-picker__email">{email}</span>}
                        </div>
                        <button
                          type="button"
                          className="club-admins-modal__add"
                          disabled={alreadyAdded}
                          onClick={() => addPerson(person)}
                        >
                          {alreadyAdded
                            ? t('clubs.admins.added', 'Added')
                            : t('clubs.admins.add', 'Add')}
                        </button>
                      </li>
                    );
                  })
                )}
              </ul>
            )}
          </div>
        </section>

        <footer className="club-admins-modal__footer">
          <button type="button" className="club-admins-modal__done" onClick={onClose}>
            {t('common.done', 'Done')}
          </button>
        </footer>
      </div>
    </div>
  );
}

export default ClubAdminsPicker;
