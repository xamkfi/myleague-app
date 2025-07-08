import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballRefereeService, type FloorballRefereeDto } from '../../../../../api/floorball/floorballRefereeService';
import type { Person } from '../../../../../types/admin/personTypes';
import './CreateRefereeModal.scss';

interface CreateRefereeModalProps {
  isOpen: boolean;
  onClose: () => void;
  onRefereeCreated: (newReferee: FloorballRefereeDto) => void;
}

const CreateRefereeModal = ({
  isOpen,
  onClose,
  onRefereeCreated
}: CreateRefereeModalProps) => {
  const { t } = useTranslation();
  const [availablePersons, setAvailablePersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<number | null>(null);
  const [selectedPersonId, setSelectedPersonId] = useState<string>('');
  const [licenseIssueDate, setLicenseIssueDate] = useState('');
  const [licenseExpiryDate, setLicenseExpiryDate] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);

  // Fetch persons and filter out those who are already referees
  useEffect(() => {
    const fetchData = async () => {
      if (!isOpen) return;

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
  }, [isOpen]);

  // Reset state when modal closes
  useEffect(() => {
    if (!isOpen) {
      setAvailablePersons([]);
      setError(null);
      setSearchTerm('');
      setSuccessMessage(null);
      setSelectedPersonId('');
      setLicenseIssueDate('');
      setLicenseExpiryDate('');
      setShowCreateForm(false);
      // Clear any existing timeout when modal closes
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
        setSuccessTimeoutId(null);
      }
    }
  }, [isOpen, successTimeoutId]);

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
      const newReferee = await floorballRefereeService.create({ 
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
      
      // Auto-hide success message after 3 seconds
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
      }, 3000);
      setSuccessTimeoutId(timeoutId);
      
      onRefereeCreated(newReferee);
      
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

  if (!isOpen) return null;

  const selectedPerson = availablePersons.find(p => p.id === selectedPersonId);

  return (
    <div className="modal-overlay" onClick={onClose}>
      {/* Floating Success Toast */}
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>
            {showCreateForm 
              ? t('floorball.referees.createRefereeFor', 'Create Referee for {{name}}', { name: selectedPerson?.fullName })
              : t('floorball.referees.createFromPerson', 'Create Referee from Available Persons')
            }
          </h2>
          <button
            className="modal-close"
            onClick={onClose}
            type="button"
            aria-label="Close modal"
          >
            ×
          </button>
        </div>

        <div className="modal-body">
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

              {/* Loading State */}
              {loading && (
                <div className="loading-container">
                  <p>{t('common.loading', 'Loading...')}</p>
                </div>
              )}

              {/* Error Message */}
              {error && (
                <div className="error-message">
                  <p>{error}</p>
                </div>
              )}

              {/* Persons List */}
              {!loading && !error && (
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
              )}
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

        <div className="modal-footer">
          <button
            type="button"
            onClick={onClose}
            className="cancel-button"
            disabled={creating}
          >
            {t('common.cancel', 'Cancel')}
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateRefereeModal; 