import { useEffect, useState, useMemo, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import type { SystemUser, UserRole } from '../../../../types/admin/userTypes';
import type { Person } from '../../../../types/admin/personTypes';
import { personApi } from '../../../../api/admin/personApi';

const MAX_PAGE_SIZE = 50;

interface UserFormModalProps {
  isOpen: boolean;
  /** When set, the modal is in edit mode for this user */
  user: SystemUser | null;
  /** IDs of persons that already have a user account (to show badge / disable) */
  existingPersonIds: string[];
  onSave: (email: string, personId: string, role: UserRole) => Promise<void>;
  onCancel: () => void;
  onResendInvitation?: (user: SystemUser) => void;
}

const UserFormModal = ({
  isOpen,
  user,
  existingPersonIds,
  onSave,
  onCancel,
  onResendInvitation,
}: UserFormModalProps) => {
  const { t } = useTranslation();
  const isEditMode = user !== null;

  const [email, setEmail] = useState('');
  const [role, setRole] = useState<UserRole>('ClubAdmin');
  const [selectedPersonId, setSelectedPersonId] = useState('');
  const [personSearch, setPersonSearch] = useState('');
  const [persons, setPersons] = useState<Person[]>([]);
  const [loadingPersons, setLoadingPersons] = useState(false);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<{ email?: string; person?: string }>({});

  const debounceTimerRef = useRef<number | null>(null);

  // Reset form state when the modal opens / user changes
  useEffect(() => {
    if (isOpen) {
      setEmail(user?.email ?? '');
      setRole(user?.role ?? 'ClubAdmin');
      setSelectedPersonId(user?.personId ?? '');
      setPersonSearch('');
      setErrors({});
      setSaving(false);

      // Load persons for the selector (only in create mode)
      if (!user) {
        fetchPersons('');
      }
    }

    return () => {
      if (debounceTimerRef.current !== null) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, [isOpen, user]);

  // Fetch persons from backend — uses search API when a term is provided, getAll otherwise
  const fetchPersons = useCallback(async (searchTerm: string) => {
    try {
      setLoadingPersons(true);
      const trimmed = searchTerm.trim();

      let allPersons: Person[] = [];

      if (trimmed.length >= 2) {
        // Use server-side search
        const response = await personApi.search(trimmed, 1, MAX_PAGE_SIZE);
        allPersons = response.data ?? [];
      } else {
        // Load first page; if more pages exist, fetch them in parallel
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

  // Handle search input — debounce and call server-side search
  const handleSearchChange = (value: string) => {
    setPersonSearch(value);

    if (debounceTimerRef.current !== null) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = window.setTimeout(() => {
      fetchPersons(value);
    }, 300);
  };

  const existingSet = useMemo(() => new Set(existingPersonIds), [existingPersonIds]);

  const validate = (): boolean => {
    const newErrors: { email?: string; person?: string } = {};

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

    try {
      setSaving(true);
      await onSave(email.trim(), selectedPersonId, role);
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
            {/* Person selector (create mode only) */}
            {!isEditMode && (
              <div className="user-form-group">
                <label htmlFor="personSelect">
                  {t('admin.users.form.selectPerson', 'Person')} *
                </label>
                <input
                  type="text"
                  className="user-form-search"
                  placeholder={t(
                    'admin.users.form.searchPerson',
                    'Search persons...',
                  )}
                  value={personSearch}
                  onChange={(e) => handleSearchChange(e.target.value)}
                />
                <div className="user-form-person-list">
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
                          className={`user-form-person-item ${selectedPersonId === person.id ? 'selected' : ''} ${hasAccount ? 'disabled' : ''}`}
                          onClick={() => {
                            if (!hasAccount) {
                              setSelectedPersonId(person.id);
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
                    >
                      {t('admin.users.actions.resendInvitation', 'Resend Invitation')}
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
