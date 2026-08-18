import { useEffect, useState, useMemo, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import type { SystemUser, UserRole } from '../../../../types/admin/userTypes';
import type { Person } from '../../../../types/admin/personTypes';
import { personApi } from '../../../../api/admin/personApi';
import { clubService } from '../../../../api/common/clubService';

const MAX_PAGE_SIZE = 50;

interface ClubOption {
  id: string;
  name: string;
}

interface UserFormModalProps {
  isOpen: boolean;
  user: SystemUser | null;
  existingPersonIds: string[];
  onSave: (
    email: string,
    personId: string,
    role: UserRole,
    isActive: boolean,
    clubAssignments?: string[],
  ) => Promise<void>;
  onCancel: () => void;
  onResendInvitation?: (user: SystemUser) => void;
  isResendingInvitation?: boolean;
}

const UserFormModal = ({
  isOpen,
  user,
  existingPersonIds,
  onSave,
  onCancel,
  onResendInvitation,
  isResendingInvitation = false,
}: UserFormModalProps) => {
  const { t } = useTranslation();
  const isEditMode = user !== null;

  const [email, setEmail] = useState('');
  const [role, setRole] = useState<UserRole>('ClubAdmin');
  const [isActive, setIsActive] = useState(true);
  const [selectedPersonId, setSelectedPersonId] = useState('');
  const [selectedPersonName, setSelectedPersonName] = useState('');
  const [personSearch, setPersonSearch] = useState('');
  const [persons, setPersons] = useState<Person[]>([]);
  const [loadingPersons, setLoadingPersons] = useState(false);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<{ email?: string; person?: string; clubs?: string }>({});

  // Club admin club assignment state (create mode only)
  const [clubs, setClubs] = useState<ClubOption[]>([]);
  const [clubsLoaded, setClubsLoaded] = useState(false);
  const [clubFilter, setClubFilter] = useState('');
  const [selectedClubIds, setSelectedClubIds] = useState<Set<string>>(new Set());

  const debounceTimerRef = useRef<number | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const modalRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isOpen) {
      setEmail(user?.email ?? '');
      setRole(user?.role ?? 'ClubAdmin');
      setIsActive(user?.isActive ?? true);
      setSelectedPersonId(user?.personId ?? '');
      setSelectedPersonName(user?.person?.fullName ?? '');
      setPersonSearch('');
      setPersons([]);
      setIsDropdownOpen(false);
      setErrors({});
      setSaving(false);
      setClubFilter('');
      setSelectedClubIds(new Set());

      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }

    return () => {
      document.body.style.overflow = '';
      if (debounceTimerRef.current !== null) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, [isOpen, user]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (isDropdownOpen) {
          setIsDropdownOpen(false);
        } else {
          onCancel();
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isDropdownOpen, onCancel]);

  useEffect(() => {
    if (!isOpen || !isDropdownOpen) return;

    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setIsDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isOpen, isDropdownOpen]);

  // Load all club names for the club admin picker once, when the role is first selected.
  useEffect(() => {
    if (!isOpen || isEditMode || role !== 'ClubAdmin' || clubsLoaded) return;

    let cancelled = false;
    const loadClubs = async () => {
      try {
        const allClubs = await clubService.getAll();
        if (cancelled) return;
        setClubs(allClubs.map((club) => ({ id: club.id, name: club.name })));
        setClubsLoaded(true);
      } catch (err) {
        console.error('Failed to load clubs for club admin assignment:', err);
      }
    };

    void loadClubs();
    return () => { cancelled = true; };
  }, [isOpen, isEditMode, role, clubsLoaded]);

  const toggleClubSelection = (clubId: string) => {
    setSelectedClubIds((prev) => {
      const next = new Set(prev);
      if (next.has(clubId)) {
        next.delete(clubId);
      } else {
        next.add(clubId);
      }
      return next;
    });
    if (errors.clubs) {
      setErrors((prev) => ({ ...prev, clubs: undefined }));
    }
  };

  const fetchPersons = useCallback(async (searchTerm: string) => {
    try {
      setLoadingPersons(true);
      const trimmed = searchTerm.trim();

      let allPersons: Person[] = [];

      if (trimmed.length >= 2) {
        const response = await personApi.search(trimmed, 1, MAX_PAGE_SIZE);
        allPersons = response.data ?? [];
      } else {
        const firstPage = await personApi.getAll(1, MAX_PAGE_SIZE);
        allPersons = firstPage.data ?? [];

        if (firstPage.pagination && firstPage.pagination.totalPages > 1) {
          const fetchPromises = [];
          for (let page = 2; page <= firstPage.pagination.totalPages; page++) {
            fetchPromises.push(personApi.getAll(page, MAX_PAGE_SIZE));
          }
          const pages = await Promise.all(fetchPromises);
          for (const p of pages) {
            allPersons = allPersons.concat(p.data ?? []);
          }
        }
      }

      setPersons(allPersons);
    } catch (err) {
      console.error('Failed to load persons for user form:', err);
      setPersons([]);
    } finally {
      setLoadingPersons(false);
    }
  }, []);

  const handleSearchChange = (value: string) => {
    setPersonSearch(value);

    if (debounceTimerRef.current !== null) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = window.setTimeout(() => {
      fetchPersons(value);
    }, 300);
  };

  const handleSearchFocus = () => {
    setIsDropdownOpen(true);
    if (persons.length === 0 && !loadingPersons) {
      fetchPersons(personSearch);
    }
  };

  const handleSelectPerson = (person: Person) => {
    setSelectedPersonId(person.id);
    setSelectedPersonName(`${person.firstName} ${person.lastName}`);
    setPersonSearch('');
    setIsDropdownOpen(false);
    if (errors.person) {
      setErrors((prev) => ({ ...prev, person: undefined }));
    }
  };

  const handleClearPerson = () => {
    setSelectedPersonId('');
    setSelectedPersonName('');
    setPersonSearch('');
    setPersons([]);
  };

  const existingSet = useMemo(() => new Set(existingPersonIds), [existingPersonIds]);

  const validate = (): boolean => {
    const newErrors: { email?: string; person?: string; clubs?: string } = {};

    if (!email.trim()) {
      newErrors.email = t('admin.users.form.emailRequired', 'Email is required');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      newErrors.email = t('admin.users.form.emailInvalid', 'Invalid email format');
    }

    if (!isEditMode && !selectedPersonId) {
      newErrors.person = t('admin.users.form.personRequired', 'Person is required');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    const clubAssignments: string[] | undefined =
      !isEditMode && role === 'ClubAdmin' && selectedClubIds.size > 0
        ? Array.from(selectedClubIds)
        : undefined;

    try {
      setSaving(true);
      await onSave(email.trim(), selectedPersonId, role, isActive, clubAssignments);
    } catch {
      // Parent handles the error
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="user-modal__overlay" onClick={onCancel} role="presentation">
      <div
        ref={modalRef}
        className="user-modal user-modal--form"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
      >
        <header className="user-modal__header">
          <h3>
            {isEditMode
              ? t('admin.users.form.editTitle', 'Edit User')
              : t('admin.users.form.createTitle', 'Add User')}
          </h3>
        </header>

        <form onSubmit={handleSubmit}>
          <section className="user-modal__body">
            {/* Person selector (create mode only) — dropdown combobox */}
            {!isEditMode && (
              <div className="user-form-group">
                <label>
                  {t('admin.users.form.selectPerson', 'Person')} *
                </label>

                {selectedPersonId ? (
                  <div className="user-form-person-selected">
                    <span className="user-form-person-selected__name">{selectedPersonName}</span>
                    <button
                      type="button"
                      className="user-form-person-selected__clear"
                      onClick={handleClearPerson}
                      aria-label={t('common.clear', 'Clear')}
                    >
                      &times;
                    </button>
                  </div>
                ) : (
                  <div className="user-form-person-picker" ref={dropdownRef}>
                    <input
                      ref={searchInputRef}
                      type="text"
                      className={`user-form-search ${errors.person ? 'user-form-input--error' : ''}`}
                      placeholder={t(
                        'admin.users.form.searchPerson',
                        'Search persons...',
                      )}
                      value={personSearch}
                      onChange={(e) => handleSearchChange(e.target.value)}
                      onFocus={handleSearchFocus}
                      autoFocus
                    />

                    {isDropdownOpen && (
                      <div className="user-form-person-dropdown">
                        {loadingPersons && (
                          <div className="user-form-person-list__loading">
                            {t('common.loading', 'Loading...')}
                          </div>
                        )}
                        {!loadingPersons && persons.length === 0 && (
                          <div className="user-form-person-list__empty">
                            {t(
                              'admin.users.form.noPerson',
                              'No available persons found',
                            )}
                          </div>
                        )}
                        {!loadingPersons &&
                          persons.map((person) => {
                            const hasAccount = existingSet.has(person.id);
                            return (
                              <button
                                key={person.id}
                                type="button"
                                className={`user-form-person-item ${hasAccount ? 'disabled' : ''}`}
                                onClick={() => {
                                  if (!hasAccount) {
                                    handleSelectPerson(person);
                                  }
                                }}
                                disabled={hasAccount}
                              >
                                <span className="person-item-name">
                                  {person.firstName} {person.lastName}
                                </span>
                                {hasAccount && (
                                  <span className="person-item-badge">
                                    {t('admin.users.form.hasAccount', 'Already has an account')}
                                  </span>
                                )}
                              </button>
                            );
                          })}
                      </div>
                    )}
                  </div>
                )}

                {errors.person && (
                  <span className="user-form-error">{errors.person}</span>
                )}
              </div>
            )}

            {/* Read-only person info in edit mode */}
            {isEditMode && user && (
              <div className="user-form-group">
                <label>{t('admin.users.form.selectPerson', 'Person')}</label>
                <div className="user-form-readonly">
                  {user.person?.fullName ?? '—'}
                </div>
              </div>
            )}

            {/* Email verification status in edit mode */}
            {isEditMode && user && (
              <div className="user-form-group">
                <label>{t('admin.users.form.emailVerified', 'Email Verified')}</label>
                <div className="user-form-verification-row">
                  <span className={`user-form-verification-badge ${user.isEmailVerified ? 'user-form-verification-badge--verified' : 'user-form-verification-badge--pending'}`}>
                    {user.isEmailVerified
                      ? t('admin.users.table.verified', 'Verified')
                      : t('admin.users.table.pending', 'Pending')}
                  </span>
                  {!user.isEmailVerified && onResendInvitation && (
                    <button
                      type="button"
                      className="user-form-resend-btn"
                      onClick={() => onResendInvitation(user)}
                      disabled={isResendingInvitation}
                    >
                      {isResendingInvitation
                        ? t('admin.users.actions.sendingInvitation', 'Sending...')
                        : t('admin.users.actions.resendInvitation', 'Resend Invitation')}
                    </button>
                  )}
                </div>
              </div>
            )}

            {/* Email input */}
            <div className="user-form-group">
              <label htmlFor="userEmail">
                {t('admin.users.form.email', 'Email')} *
              </label>
              <input
                id="userEmail"
                type="email"
                className={`user-form-input ${errors.email ? 'user-form-input--error' : ''}`}
                placeholder={t(
                  'admin.users.form.emailPlaceholder',
                  'user@example.com',
                )}
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (errors.email) {
                    setErrors((prev) => ({ ...prev, email: undefined }));
                  }
                }}
                autoFocus={isEditMode}
              />
              {errors.email && (
                <span className="user-form-error">{errors.email}</span>
              )}
            </div>

            {/* Role selector */}
            <div className="user-form-group">
              <label htmlFor="userRole">
                {t('admin.users.form.role', 'Role')} *
              </label>
              <select
                id="userRole"
                className="user-form-select"
                value={role}
                onChange={(e) => setRole(e.target.value as UserRole)}
              >
                <option value="ClubAdmin">
                  {t('admin.users.roles.clubAdmin', 'Club Admin')}
                </option>
                <option value="SystemAdmin">
                  {t('admin.users.roles.systemAdmin', 'System Admin')}
                </option>
              </select>
            </div>

            {/* Club assignments for new club admins */}
            {!isEditMode && role === 'ClubAdmin' && (
              <div className="user-form-group">
                <label>
                  {t('admin.users.form.clubs', 'Clubs to manage')}
                </label>
                <div className="user-form-team-picker">
                  <input
                    type="text"
                    className="user-form-input"
                    placeholder={t('admin.users.form.clubFilterPlaceholder', 'Filter clubs...')}
                    value={clubFilter}
                    onChange={(e) => setClubFilter(e.target.value)}
                  />
                  {!clubsLoaded ? (
                    <div className="user-form-team-picker__loading">
                      {t('common.loading', 'Loading...')}
                    </div>
                  ) : (
                    <div className="user-form-team-picker__lists">
                      <div className="user-form-team-picker__section">
                        <div className="user-form-team-picker__title">
                          {t('admin.users.form.clubsTitle', 'Clubs')}
                          {selectedClubIds.size > 0 && (
                            <span className="user-form-team-picker__count">{selectedClubIds.size}</span>
                          )}
                        </div>
                        <div className="user-form-team-picker__list">
                          {(clubFilter.trim()
                            ? clubs.filter((club) =>
                                club.name.toLowerCase().includes(clubFilter.trim().toLowerCase()))
                            : clubs
                          ).map((club) => (
                            <label key={club.id} className="user-form-team-picker__item">
                              <input
                                type="checkbox"
                                checked={selectedClubIds.has(club.id)}
                                onChange={() => toggleClubSelection(club.id)}
                              />
                              <span>{club.name}</span>
                            </label>
                          ))}
                          {clubs.length === 0 && (
                            <div className="user-form-team-picker__empty">
                              {t('admin.users.form.noClubsFound', 'No clubs found')}
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  )}
                </div>
                <div className="user-form-status-info">
                  {t(
                    'admin.users.form.clubsHint',
                    'You can also assign club admins later from the club edit page.',
                  )}
                </div>
                {errors.clubs && (
                  <span className="user-form-error">{errors.clubs}</span>
                )}
              </div>
            )}

            {/* IsActive toggle */}
            <div className="user-form-group">
              <label>{t('admin.users.form.status', 'Status')}</label>
              {isEditMode ? (
                <label className="user-form-toggle">
                  <input
                    type="checkbox"
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                  />
                  <span className="user-form-toggle__track" />
                  <span className="user-form-toggle__label">
                    {isActive
                      ? t('common.active', 'Active')
                      : t('common.inactive', 'Inactive')}
                  </span>
                </label>
              ) : (
                <div className="user-form-status-info">
                  {t(
                    'admin.users.form.statusCreateInfo',
                    'New users are inactive until they verify their email.',
                  )}
                </div>
              )}
            </div>
          </section>

          <footer className="user-modal__footer">
            <button
              type="button"
              className="modal-btn modal-btn--secondary"
              onClick={onCancel}
              disabled={saving}
            >
              {t('common.cancel', 'Cancel')}
            </button>
            <button
              type="submit"
              className="modal-btn modal-btn--primary"
              disabled={saving}
            >
              {saving
                ? t('admin.users.form.saving', 'Saving...')
                : t('common.save', 'Save')}
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
};

export default UserFormModal;
