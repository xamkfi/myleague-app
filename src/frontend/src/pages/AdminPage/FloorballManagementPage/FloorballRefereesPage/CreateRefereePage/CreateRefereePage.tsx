import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../../../components/BackButton/BackButton';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballRefereeService } from '../../../../../api/floorball/floorballRefereeService';
import type { Person } from '../../../../../types/admin/personTypes';
import './CreateRefereePage.scss';

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
  const [selectedPersonId, setSelectedPersonId] = useState<string>('');
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
    setSelectedPersonId(personId);
    setShowCreateForm(true);
  };

  const handleBackToList = () => {
    setShowCreateForm(false);
    setSelectedPersonId('');
    setLicenseIssueDate('');
    setLicenseExpiryDate('');
    setError(null);
  };

  const handleCreateReferee = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!selectedPersonId || !licenseIssueDate || !licenseExpiryDate) {
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
      
      // Find the person to get their name for the success message
      const person = availablePersons.find(p => p.id === selectedPersonId);
      if (!person) {
        setError('Person not found');
        return;
      }
      
      // Create the referee using the floorball referee service
      await floorballRefereeService.create({ 
        PersonId: selectedPersonId,
        LicenseIssueDate: licenseIssueDate,
        LicenseExpiryDate: licenseExpiryDate
      });
      
      // Remove the person from available persons list
      setAvailablePersons(prev => prev.filter(p => p.id !== selectedPersonId));
      
      // Clear any existing timeout to prevent flickering
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      
      // Show success message with person's name
      const message = t('floorball.referees.refereeCreated', '{{personName}} is now a floorball referee!', { 
        personName: person.fullName 
      });
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
      setSelectedPersonId('');
      setLicenseIssueDate('');
      setLicenseExpiryDate('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create referee');
      console.error('Error creating referee:', err);
    } finally {
      setCreating(false);
    }
  };

  const selectedPerson = availablePersons.find(p => p.id === selectedPersonId);

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
        ? t('floorball.referees.createRefereeFor', 'Create Referee for {{name}}', { name: selectedPerson?.fullName })
        : t('floorball.referees.createNew', 'Create New Referee')
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
            {/* Search Bar */}
            <div className="search-container">
              <input
                type="text"
                placeholder={t('floorball.referees.searchPersons', 'Search available persons...')}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
              />
            </div>

            {/* Error Message */}
            {error && (
              <div className="error-message">
                <p>{error}</p>
              </div>
            )}

            {/* Persons List */}
            <div className="persons-container">
              {filteredPersons.length === 0 ? (
                <div className="no-persons">
                  <p>{searchTerm ? 
                    t('floorball.referees.noPersonsFound', 'No persons found matching your search') :
                    t('floorball.referees.noPersonsAvailable', 'No available persons to convert to referees. All persons are already referees.')
                  }</p>
                </div>
              ) : (
                <div className="persons-list">
                  {filteredPersons.map((person) => (
                    <div key={person.id} className="person-item">
                      <div className="person-info">
                        <div className="person-name">{person.fullName}</div>
                        <div className="person-details">
                          <span className="birth-date">
                            {t('common.birthDate', 'Birth Date')}: {new Date(person.birthDate).toLocaleDateString()}
                          </span>
                          <span className={`registration-status ${person.isRegistered ? 'registered' : 'not-registered'}`}>
                            {person.isRegistered ? 
                              t('common.registered', 'Registered') : 
                              t('common.notRegistered', 'Not Registered')
                            }
                          </span>
                        </div>
                      </div>
                      <button
                        className="select-person-btn"
                        onClick={() => handlePersonSelect(person.id)}
                        disabled={creating}
                      >
                        {t('floorball.referees.selectPerson', 'Select')}
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </>
        ) : (
          <form onSubmit={handleCreateReferee} className="create-referee-form">
            {/* Error Message */}
            {error && (
              <div className="error-message">
                <p>{error}</p>
              </div>
            )}

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
                className="back-to-list-button"
                disabled={creating}
              >
                {t('common.cancelSelection', 'Cancel Selection')}
              </button>
              <button
                type="submit"
                className="create-referee-btn"
                disabled={creating}
              >
                {creating ? 
                  t('common.creating', 'Creating...') : 
                  t('floorball.referees.createReferee', 'Create Referee')
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
