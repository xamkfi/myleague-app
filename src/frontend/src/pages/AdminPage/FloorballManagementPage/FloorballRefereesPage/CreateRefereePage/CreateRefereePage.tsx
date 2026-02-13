import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballRefereeService } from '../../../../../api/floorball/floorballRefereeService';
import type { Person, PaginatedApiResponse } from '../../../../../types/admin/personTypes';
import Pagination from '../../../../../components/Pagination/Pagination';
import './CreateRefereePage.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

const CreateRefereePage = () => {
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
  const [showCreateForm, setShowCreateForm] = useState(false);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [existingRefereePersonIds, setExistingRefereePersonIds] = useState<Set<string>>(new Set());

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
      setCurrentPage(1); // Reset to first page on new search
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Fetch existing referee person IDs once on mount
  useEffect(() => {
    const fetchExistingReferees = async () => {
      try {
        const refereesResponse = await floorballRefereeService.getAll({ pageSize: 50 });
        const ids = new Set(
          (refereesResponse.data || []).map(referee => referee.personId)
        );
        setExistingRefereePersonIds(ids);
      } catch (err) {
        console.error('Error fetching existing referees:', err);
      }
    };
    fetchExistingReferees();
  }, []);

  // Fetch persons with server-side pagination
  const fetchPersons = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      let response: PaginatedApiResponse<Person>;

      if (debouncedSearchTerm.trim()) {
        response = await personApi.search(
          debouncedSearchTerm.trim(),
          currentPage,
          pageSize
        );
      } else {
        response = await personApi.getAll(currentPage, pageSize);
      }

      // Filter out persons who are already referees
      const availablePersonsData = response.data.filter(
        person => !existingRefereePersonIds.has(person.id)
      );

      setAvailablePersons(availablePersonsData);
      setTotalCount(response.pagination.totalCount);
      setTotalPages(response.pagination.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
      console.error('Error fetching data:', err);
      setAvailablePersons([]);
    } finally {
      setLoading(false);
      if (isFirstLoad.current) {
        setInitialLoading(false);
        isFirstLoad.current = false;
      }
    }
  }, [debouncedSearchTerm, currentPage, pageSize, existingRefereePersonIds]);

  useEffect(() => {
    fetchPersons();
  }, [fetchPersons]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  // Use availablePersons directly (filtering is handled by backend when searching)
  const filteredPersons = availablePersons;

  // Set default dates (issue today, expire in 2 years)
  useEffect(() => {
    if (showCreateForm && !licenseIssueDate) {
      const today = new Date();
      const twoYearsFromNow = new Date();
      twoYearsFromNow.setFullYear(today.getFullYear() + 2);
      
      setLicenseIssueDate(today.toISOString().split('T')[0]);
      setLicenseExpiryDate(twoYearsFromNow.toISOString().split('T')[0]);
    }
  }, [showCreateForm, licenseIssueDate]);

  const handlePersonSelect = (personId: string) => {
    setSelectedPersonIds(prev => {
      const newSet = new Set(prev);
      if (newSet.has(personId)) {
        newSet.delete(personId);
      } else {
        newSet.add(personId);
      }
      return newSet;
    });
  };

  const handleSelectAll = () => {
    setSelectedPersonIds(new Set(filteredPersons.map(p => p.id)));
  };

  const handleClearSelection = () => {
    setSelectedPersonIds(new Set());
  };

  const handleProceedToForm = () => {
    if (selectedPersonIds.size > 0) {
      setShowCreateForm(true);
    }
  };

  const handleBackToList = () => {
    setShowCreateForm(false);
    setLicenseIssueDate('');
    setLicenseExpiryDate('');
    setError(null);
  };

  const handleCreateReferee = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (selectedPersonIds.size === 0 || !licenseIssueDate || !licenseExpiryDate) {
      setError('All fields are required');
      return;
    }

    // Validate dates
    const issueDate = new Date(licenseIssueDate);
    const expiryDate = new Date(licenseExpiryDate);
    
    if (expiryDate <= issueDate) {
      setError('License expiry date must be after the issue date');
      return;
    }

    try {
      setCreating(true);
      setError(null);
      setSuccessMessage(null);
      
      let successCount = 0;
      const selectedPersonsList = availablePersons.filter(p => selectedPersonIds.has(p.id));
      
      // Create referee for each selected person
      for (const person of selectedPersonsList) {
        try {
          await floorballRefereeService.create({ 
            PersonId: person.id,
            LicenseIssueDate: licenseIssueDate,
            LicenseExpiryDate: licenseExpiryDate
          });
          successCount++;
        } catch (err) {
          console.error(`Failed to create referee for ${person.firstName} ${person.lastName}:`, err);
        }
      }
      
      // Update the existing referee IDs and remove created persons from available list
      setExistingRefereePersonIds(prev => {
        const newSet = new Set(prev);
        selectedPersonIds.forEach(id => newSet.add(id));
        return newSet;
      });
      setAvailablePersons(prev => prev.filter(p => !selectedPersonIds.has(p.id)));
      
      // Clear any existing timeout to prevent flickering
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      
      // Show success message
      const message = successCount === 1
        ? t('floorball.referees.refereeCreated', '{{count}} referee created successfully!', { count: successCount })
        : t('floorball.referees.refereesCreated', '{{count}} referees created successfully!', { count: successCount });
      setSuccessMessage(message);
      
      // Auto-hide success message after 3 seconds and then navigate back
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        navigate('/admin/floorball/referees');
      }, 3000);
      setSuccessTimeoutId(timeoutId);
      
      // Reset form
      setShowCreateForm(false);
      setSelectedPersonIds(new Set());
      setLicenseIssueDate('');
      setLicenseExpiryDate('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create referees');
      console.error('Error creating referees:', err);
    } finally {
      setCreating(false);
    }
  };

  const selectedPersons = availablePersons.filter(p => selectedPersonIds.has(p.id));

  if (initialLoading) {
    return (
      <PageTemplate title={t('floorball.referees.createNew', 'Create New Referee')}>
        <div className="create-referee-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={
      showCreateForm 
        ? t('floorball.referees.createRefereeForMultiple', 'Create Referees for {{count}} person(s)', { count: selectedPersonIds.size })
        : t('floorball.referees.createNew', 'CREATE REFEREE FROM AVAILABLE PERSONS')
    }>
      {/* Floating Success Toast */}
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      
      <div className="create-referee-container">

        {!showCreateForm ? (
          <>
            {/* Search Bar and Create Button */}
            <div className="search-header">
              <div className="search-container">
                <input
                  type="text"
                  placeholder={t('floorball.referees.searchPersons', 'Search available persons...')}
                  value={searchTerm}
                  onChange={(e) => {
                    const value = e.target.value;
                    setSearchTerm(value);
                    if (!value.trim()) {
                      setDebouncedSearchTerm('');
                    }
                  }}
                  className="search-input"
                />
              </div>
              <button className="create-person-link" onClick={() => navigate('/admin/persons/new')}>
                <span className="plus-icon">+</span>
                {t('floorball.referees.createNewPerson', 'Create new person')}
              </button>
            </div>

            <ErrorPopup message={error} />

            {/* Selection Controls */}
            <div className="selection-controls">
              <div className="selection-info">
                <span className="selected-count">
                  {t('floorball.referees.selectedCount', '{{count}} selected', { count: selectedPersonIds.size })}
                </span>
                {selectedPersonIds.size > 0 && (
                  <button
                    type="button"
                    className="clear-selection-btn"
                    onClick={handleClearSelection}
                  >
                    {t('common.clear', 'Clear')}
                  </button>
                )}
              </div>
              <div className="proceed-action">
                <button
                  onClick={handleProceedToForm}
                  className={`proceed-button ${selectedPersonIds.size === 0 ? 'disabled' : ''}`}
                  disabled={creating || selectedPersonIds.size === 0}
                >
                  {t('floorball.referees.createReferees', 'Create referee(s) ({{count}})', { count: selectedPersonIds.size })}
                </button>
              </div>
            </div>

            {/* Persons Table */}
            <div className={`persons-table-wrapper${loading ? ' is-loading' : ''}`}>
              {filteredPersons.length === 0 && !loading ? (
                <div className="no-persons">
                  <p>{searchTerm ? 
                    t('floorball.referees.noPersonsFound', 'No persons found matching your search') :
                    t('floorball.referees.noPersonsAvailable', 'No available persons to convert to referees. All persons are already referees.')
                  }</p>
                </div>
              ) : (
                <table className="persons-table">
                  <thead>
                    <tr>
                      <th className="select-column">
                        <input
                          type="checkbox"
                          checked={filteredPersons.length > 0 && filteredPersons.every(p => selectedPersonIds.has(p.id))}
                          onChange={(e) => {
                            if (e.target.checked) {
                              handleSelectAll();
                            } else {
                              handleClearSelection();
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
                    {filteredPersons.map((person) => (
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
                            onClick={(e) => e.stopPropagation()}
                            className="person-checkbox"
                          />
                        </td>
                        <td className="firstname-cell">
                          {person.firstName || '-'}
                        </td>
                        <td className="lastname-cell">
                          {person.lastName || '-'}
                        </td>
                        <td className="birthdate-cell">
                          {person.birthDate ? new Date(person.birthDate).toLocaleDateString() : '-'}
                        </td>
                        <td className="registration-cell">
                          <span className={`registration-badge ${person.isRegistered ? 'registered' : 'not-registered'}`}>
                            {person.isRegistered ? 
                              t('common.registered', 'Registered') : 
                              t('common.notRegistered', 'Not Registered')
                            }
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>

            {/* Pagination */}
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
          <form onSubmit={handleCreateReferee} className="create-referee-form">
            {/* Error moved to global ErrorPopup */}

            {/* Show selected persons */}
            <div className="selected-persons-list">
              <h3>{t('floorball.referees.selectedPersons', 'Selected persons:')}</h3>
              <ul>
                {selectedPersons.map(person => (
                  <li key={person.id}>{[person.firstName, person.lastName].filter(Boolean).join(' ') || '-'}</li>
                ))}
              </ul>
            </div>

            <div className="form-group">
              <label htmlFor="licenseIssueDate">
                {t('floorball.referees.licenseIssueDate', 'License Issue Date')}
              </label>
              <input
                type="date"
                id="licenseIssueDate"
                value={licenseIssueDate}
                onChange={(e) => setLicenseIssueDate(e.target.value)}
                required
                disabled={creating}
              />
            </div>

            <div className="form-group">
              <label htmlFor="licenseExpiryDate">
                {t('floorball.referees.licenseExpiryDate', 'License Expiry Date')}
              </label>
              <input
                type="date"
                id="licenseExpiryDate"
                value={licenseExpiryDate}
                onChange={(e) => setLicenseExpiryDate(e.target.value)}
                required
                disabled={creating}
              />
            </div>

            <div className="form-actions">
              <button
                type="button"
                onClick={handleBackToList}
                className="cancel-button"
                disabled={creating}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="submit"
                className="create-referee-button"
                disabled={creating}
              >
                {creating ? 
                  t('common.creating', 'Creating...') : 
                  t('floorball.referees.createReferee', 'Create referee')
                }
              </button>
            </div>
          </form>
        )}
      </div>
    </PageTemplate>
  );
};

export default CreateRefereePage;
