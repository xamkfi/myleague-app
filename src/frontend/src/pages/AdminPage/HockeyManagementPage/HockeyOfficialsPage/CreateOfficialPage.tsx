import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { personApi } from '../../../../api/admin/personApi';
import { hockeyOfficialService } from '../../../../api/hockey/hockeyOfficialService';
import { HOCKEY_OFFICIAL_ROLES, type HockeyOfficialRole } from '../../../../types/hockey/hockeyTypes';
import type { Person, PaginatedApiResponse } from '../../../../types/admin/personTypes';
import Pagination from '../../../../components/Pagination/Pagination';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './CreateOfficialPage/CreateOfficialPage.scss';

function CreateHockeyOfficialPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [availablePersons, setAvailablePersons] = useState<Person[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const isFirstLoad = useRef(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  const [selectedPersonIds, setSelectedPersonIds] = useState<Set<string>>(new Set());
  const [licenseIssueDate, setLicenseIssueDate] = useState('');
  const [licenseExpiryDate, setLicenseExpiryDate] = useState('');
  const [officialRole, setOfficialRole] = useState<HockeyOfficialRole>('Referee');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [existingOfficialPersonIds, setExistingOfficialPersonIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
      setCurrentPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    const fetchExisting = async (): Promise<void> => {
      try {
        const officials = await hockeyOfficialService.getAll();
        setExistingOfficialPersonIds(new Set(officials.map((item) => item.personId)));
      } catch (err) {
        console.error('Error fetching existing officials:', err);
      }
    };
    void fetchExisting();
  }, []);

  const fetchPersons = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);
      const response: PaginatedApiResponse<Person> = debouncedSearchTerm.trim()
        ? await personApi.search(debouncedSearchTerm.trim(), currentPage, pageSize)
        : await personApi.getAll(currentPage, pageSize);
      const available = response.data.filter((person) => !existingOfficialPersonIds.has(person.id));
      setAvailablePersons(available);
      setTotalCount(response.pagination.totalCount);
      setTotalPages(response.pagination.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
      setAvailablePersons([]);
    } finally {
      setLoading(false);
      if (isFirstLoad.current) {
        setInitialLoading(false);
        isFirstLoad.current = false;
      }
    }
  }, [debouncedSearchTerm, currentPage, pageSize, existingOfficialPersonIds]);

  useEffect(() => {
    void fetchPersons();
  }, [fetchPersons]);

  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  useEffect(() => {
    if (showCreateForm && !licenseIssueDate) {
      const today = new Date();
      const twoYearsFromNow = new Date();
      twoYearsFromNow.setFullYear(today.getFullYear() + 2);
      setLicenseIssueDate(today.toISOString().split('T')[0]);
      setLicenseExpiryDate(twoYearsFromNow.toISOString().split('T')[0]);
    }
  }, [showCreateForm, licenseIssueDate]);

  const handlePersonSelect = (personId: string): void => {
    setSelectedPersonIds((prev) => {
      const next = new Set(prev);
      if (next.has(personId)) {
        next.delete(personId);
      } else {
        next.add(personId);
      }
      return next;
    });
  };

  const handleCreateOfficial = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    if (selectedPersonIds.size === 0 || !licenseIssueDate || !licenseExpiryDate) {
      setError(t('hockey.officials.validation.allRequired', 'All fields are required'));
      return;
    }
    const issueDate = new Date(licenseIssueDate);
    const expiryDate = new Date(licenseExpiryDate);
    if (expiryDate <= issueDate) {
      setError(t('hockey.officials.validation.expiryAfterIssue', 'License expiry date must be after the issue date'));
      return;
    }
    try {
      setCreating(true);
      setError(null);
      let successCount = 0;
      const selectedPersonsList = availablePersons.filter((person) => selectedPersonIds.has(person.id));
      for (const person of selectedPersonsList) {
        try {
          await hockeyOfficialService.create({
            personId: person.id,
            officialRole,
            licenseIssueDate,
            licenseExpiryDate,
          });
          successCount += 1;
        } catch (err) {
          console.error(`Failed to create official for ${person.firstName} ${person.lastName}:`, err);
        }
      }
      setExistingOfficialPersonIds((prev) => {
        const next = new Set(prev);
        selectedPersonIds.forEach((id) => next.add(id));
        return next;
      });
      setAvailablePersons((prev) => prev.filter((person) => !selectedPersonIds.has(person.id)));
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      const message = successCount === 1
        ? t('hockey.officials.officialCreated', '{{count}} referee created successfully!', { count: successCount })
        : t('hockey.officials.officialsCreated', '{{count}} referees created successfully!', { count: successCount });
      setSuccessMessage(message);
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        navigate('/admin/hockey/officials');
      }, 3000);
      setSuccessTimeoutId(timeoutId);
      setShowCreateForm(false);
      setSelectedPersonIds(new Set());
      setLicenseIssueDate('');
      setLicenseExpiryDate('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create referees');
    } finally {
      setCreating(false);
    }
  };

  const selectedPersons = availablePersons.filter((person) => selectedPersonIds.has(person.id));

  if (initialLoading) {
    return (
      <PageTemplate title={t('hockey.officials.create', 'Create New Referee')}>
        <div className="create-referee-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate
      title={
        showCreateForm
          ? t('hockey.officials.createRefereeForMultiple', 'Create Referees for {{count}} person(s)', { count: selectedPersonIds.size })
          : t('hockey.officials.createFromPersons', 'CREATE REFEREE FROM AVAILABLE PERSONS')
      }
    >
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      <div className="create-referee-container">
        {!showCreateForm ? (
          <>
            <div className="search-header">
              <div className="search-container">
                <input
                  type="text"
                  placeholder={t('hockey.officials.searchPersons', 'Search available persons...')}
                  value={searchTerm}
                  onChange={(event) => {
                    const value = event.target.value;
                    setSearchTerm(value);
                    if (!value.trim()) {
                      setDebouncedSearchTerm('');
                    }
                  }}
                  className="search-input"
                />
              </div>
              <button className="create-person-link" type="button" onClick={() => navigate('/admin/hockey/players/create-person')}>
                <span className="plus-icon">+</span>
                {t('hockey.officials.createNewPerson', 'Create new person')}
              </button>
            </div>
            <ErrorPopup message={error} />
            <div className="selection-controls">
              <div className="selection-info">
                <span className="selected-count">
                  {t('hockey.officials.selectedCount', '{{count}} selected', { count: selectedPersonIds.size })}
                </span>
                {selectedPersonIds.size > 0 && (
                  <button type="button" className="clear-selection-btn" onClick={() => setSelectedPersonIds(new Set())}>
                    {t('common.clear', 'Clear')}
                  </button>
                )}
              </div>
              <div className="proceed-action">
                <button
                  type="button"
                  onClick={() => {
                    if (selectedPersonIds.size > 0) {
                      setShowCreateForm(true);
                    }
                  }}
                  className={`proceed-button ${selectedPersonIds.size === 0 ? 'disabled' : ''}`}
                  disabled={creating || selectedPersonIds.size === 0}
                >
                  {t('hockey.officials.createReferees', 'Create referee(s) ({{count}})', { count: selectedPersonIds.size })}
                </button>
              </div>
            </div>
            <div className={`persons-table-wrapper${loading ? ' is-loading' : ''}`}>
              {availablePersons.length === 0 && !loading ? (
                <div className="no-persons">
                  <p>
                    {searchTerm
                      ? t('hockey.officials.noPersonsFound', 'No persons found matching your search')
                      : t('hockey.officials.noPersonsAvailable', 'No available persons to convert to referees.')}
                  </p>
                </div>
              ) : (
                <table className="persons-table">
                  <thead>
                    <tr>
                      <th className="select-column">
                        <input
                          type="checkbox"
                          checked={availablePersons.length > 0 && availablePersons.every((person) => selectedPersonIds.has(person.id))}
                          onChange={(event) => {
                            if (event.target.checked) {
                              setSelectedPersonIds(new Set(availablePersons.map((person) => person.id)));
                            } else {
                              setSelectedPersonIds(new Set());
                            }
                          }}
                          title={t('common.selectAll', 'Select all')}
                        />
                      </th>
                      <th className="firstname-column">{t('common.firstName', 'First Name')}</th>
                      <th className="lastname-column">{t('common.lastName', 'Last Name')}</th>
                      <th className="birthdate-column">{t('common.birthDate', 'Birth Date')}</th>
                      <th className="registration-column">{t('common.registration', 'Registration')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {availablePersons.map((person) => (
                      <tr
                        key={person.id}
                        className={selectedPersonIds.has(person.id) ? 'selected' : ''}
                        onClick={() => handlePersonSelect(person.id)}
                      >
                        <td className="select-cell">
                          <input
                            type="checkbox"
                            checked={selectedPersonIds.has(person.id)}
                            onChange={() => handlePersonSelect(person.id)}
                            onClick={(event) => event.stopPropagation()}
                            className="person-checkbox"
                          />
                        </td>
                        <td className="firstname-cell">{person.firstName || '-'}</td>
                        <td className="lastname-cell">{person.lastName || '-'}</td>
                        <td className="birthdate-cell">
                          {person.birthDate ? new Date(person.birthDate).toLocaleDateString() : '-'}
                        </td>
                        <td className="registration-cell">
                          <span className={`registration-badge ${person.isRegistered ? 'registered' : 'not-registered'}`}>
                            {person.isRegistered ? t('common.registered', 'Registered') : t('common.notRegistered', 'Not Registered')}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            {totalPages > 0 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={(newSize) => {
                  setPageSize(newSize);
                  setCurrentPage(1);
                }}
                pageSizeOptions={[10, 25, 50]}
                className="compact"
              />
            )}
          </>
        ) : (
          <form onSubmit={(event) => void handleCreateOfficial(event)} className="create-referee-form">
            <div className="selected-persons-list">
              <h3>{t('hockey.officials.selectedPersons', 'Selected persons:')}</h3>
              <ul>
                {selectedPersons.map((person) => (
                  <li key={person.id}>{[person.firstName, person.lastName].filter(Boolean).join(' ') || '-'}</li>
                ))}
              </ul>
            </div>
            <div className="form-group">
              <label htmlFor="officialRole">{t('hockey.officials.role', 'Role')}</label>
              <select
                id="officialRole"
                value={officialRole}
                onChange={(event) => setOfficialRole(event.target.value as HockeyOfficialRole)}
                disabled={creating}
              >
                {HOCKEY_OFFICIAL_ROLES.map((role) => (
                  <option key={role} value={role}>
                    {t(`hockey.officials.roles.${role}`, role)}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="licenseIssueDate">{t('hockey.officials.licenseIssueDate', 'License Issue Date')}</label>
              <input
                type="date"
                id="licenseIssueDate"
                value={licenseIssueDate}
                onChange={(event) => setLicenseIssueDate(event.target.value)}
                required
                disabled={creating}
              />
            </div>
            <div className="form-group">
              <label htmlFor="licenseExpiryDate">{t('hockey.officials.licenseExpiryDate', 'License Expiry Date')}</label>
              <input
                type="date"
                id="licenseExpiryDate"
                value={licenseExpiryDate}
                onChange={(event) => setLicenseExpiryDate(event.target.value)}
                required
                disabled={creating}
              />
            </div>
            <div className="form-actions">
              <button
                type="button"
                onClick={() => {
                  setShowCreateForm(false);
                  setLicenseIssueDate('');
                  setLicenseExpiryDate('');
                  setError(null);
                }}
                className="cancel-button"
                disabled={creating}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button type="submit" className="create-referee-button" disabled={creating}>
                {creating ? t('common.creating', 'Creating...') : t('hockey.officials.create', 'Create referee')}
              </button>
            </div>
          </form>
        )}
      </div>
    </PageTemplate>
  );
}

export default CreateHockeyOfficialPage;
