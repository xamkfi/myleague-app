import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../../../components/BackButton/BackButton';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballRefereeService } from '../../../../../api/floorball/floorballRefereeService';
import type { Person } from '../../../../../types/admin/personTypes';
import './CreateRefereePage.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

const CreateRefereePage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [availablePersons, setAvailablePersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  const [selectedPersonIds, setSelectedPersonIds] = useState<Set<string>>(new Set());
  const [licenseIssueDate, setLicenseIssueDate] = useState('');
  const [licenseExpiryDate, setLicenseExpiryDate] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);

  // Fetch persons and filter out those who are already referees
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Fetch all persons
        const personsData = await personApi.getAll();
        
        // Fetch existing referees using the same parameters that work in the main page
        const refereesResponse = await floorballRefereeService.getAll({ 
          pageSize: 50 
        });
        
        // Extract person IDs that are already referees
        const existingRefereePersonIds = new Set(
          (refereesResponse.data || []).map(referee => referee.personId)
        );
        
        // Filter out persons who are already referees
        const availablePersonsData = personsData.filter(
          person => !existingRefereePersonIds.has(person.id)
        );
        
        setAvailablePersons(availablePersonsData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load data');
        console.error('Error fetching data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  // Filter available persons based on search term
  const filteredPersons = availablePersons.filter(person =>
    person.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    person.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    person.lastName.toLowerCase().includes(searchTerm.toLowerCase())
  );

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
          console.error(`Failed to create referee for ${person.fullName}:`, err);
        }
      }
      
      // Remove the created persons from available persons list
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

  if (loading) {
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
        {/* Back button */}
        <BackButton 
          to="/admin/floorball/referees" 
          text={t('common.back', 'Back to Referees')} 
        />

        {!showCreateForm ? (
          <>
            {/* Search Bar and Create Button */}
            <div className="search-header">
              <div className="search-container">
                <input
                  type="text"
                  placeholder={t('floorball.referees.searchPersons', 'Search available persons...')}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="search-input"
                />
              </div>
              <button className="create-person-link" onClick={() => navigate('/admin/persons/create')}>
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
                  <button
                    type="button"
                    className="clear-selection-btn"
                    onClick={handleClearSelection}
                  >
                    {t('common.clear', 'Clear')}
                  </button>
                </div>
                <div className="proceed-action">
                {selectedPersonIds.size > 0 && (
                <button
                  onClick={handleProceedToForm}
                  className="proceed-button"
                  disabled={creating}
                >
                  {t('floorball.referees.createReferees', 'Create referee(s) ({{count}})', { count: selectedPersonIds.size })}
                </button>
                )}
                {selectedPersonIds.size == 0 && (
                  <button
                  onClick={handleProceedToForm}
                  className="dead-proceed-button"
                  disabled={creating}
                >
                  {t('floorball.referees.createReferees', 'Create referee(s) ({{count}})', { count: selectedPersonIds.size })}
                </button>
                )}
              </div>
              </div>

            {/* Persons Table */}
            <div className="persons-table-wrapper">
              {filteredPersons.length === 0 ? (
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
                      <th className="name-column">{t('common.name', 'Name')}</th>
                      <th className="birthdate-column">{t('common.birthDate', 'Birth date')}</th>
                      <th className="registration-column">{t('common.registration', 'Registration')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredPersons.map((person) => (
                      <tr key={person.id}>
                        <td className="select-cell">
                          <input
                            type="checkbox"
                            checked={selectedPersonIds.has(person.id)}
                            onChange={() => handlePersonSelect(person.id)}
                            className="person-checkbox"
                          />
                        </td>
                        <td className="name-cell">
                          <div className="person-name">{person.fullName}</div>
                          <div className="person-birthdate-mobile">{person.birthDate ? new Date(person.birthDate).toLocaleDateString() : '-'}</div>
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
          </>
        ) : (
          <form onSubmit={handleCreateReferee} className="create-referee-form">
            {/* Error moved to global ErrorPopup */}

            {/* Show selected persons */}
            <div className="selected-persons-list">
              <h3>{t('floorball.referees.selectedPersons', 'Selected persons:')}</h3>
              <ul>
                {selectedPersons.map(person => (
                  <li key={person.id}>{person.fullName}</li>
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
