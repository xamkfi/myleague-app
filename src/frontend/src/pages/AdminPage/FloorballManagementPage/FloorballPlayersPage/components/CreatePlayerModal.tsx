import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { Person } from '../../../../../types/admin/personTypes';
import PersonForm from '../../../PersonsPage/components/PersonForm/PersonForm';
import './CreatePlayerModal.scss';

type ModalMode = 'selectPerson' | 'createPerson';

interface CreatePlayerModalProps {
  isOpen: boolean;
  onClose: () => void;
  onPlayerCreated: (newPlayer: FloorballPlayerDto) => void;
}

const CreatePlayerModal = ({
  isOpen,
  onClose,
  onPlayerCreated
}: CreatePlayerModalProps) => {
  const { t } = useTranslation();
  const [availablePersons, setAvailablePersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<number | null>(null);
  
  // Modal mode state for switching between person selection and creation
  const [modalMode, setModalMode] = useState<ModalMode>('selectPerson');
  const [newlyCreatedPersons, setNewlyCreatedPersons] = useState<Person[]>([]);

  // Fetch persons and filter out those who are already players
  useEffect(() => {
    const fetchData = async () => {
      if (!isOpen) return;

      try {
        setLoading(true);
        setError(null);
        
        // Fetch all persons
        const personsData = await personApi.getAll();
        
        // Fetch existing players using the same parameters that work in the main page
        const playersResponse = await floorballPlayerService.getAll({ 
          pageSize: 50 
        });
        
        // Extract person IDs that are already players
        const existingPlayerPersonIds = new Set(
          (playersResponse.data || []).map(player => player.personId)
        );
        
        // Filter out persons who are already players
        const availablePersonsData = personsData.filter(
          person => !existingPlayerPersonIds.has(person.id)
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
      setModalMode('selectPerson');
      setNewlyCreatedPersons([]);
      // Clear any existing timeout when modal closes
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
        setSuccessTimeoutId(null);
      }
    }
  }, [isOpen, successTimeoutId]);

  // Filter available persons based on search term and highlight newly created ones
  const filteredPersons = availablePersons.filter(person =>
    person.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    person.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    person.lastName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const isPersonNewlyCreated = (person: Person) => 
    newlyCreatedPersons.some(newPerson => newPerson.id === person.id);

  const handleCreatePlayer = async (personId: string) => {
    try {
      setCreating(true);
      setError(null);
      setSuccessMessage(null);
      
      // Find the person to get their name for the success message
      const person = availablePersons.find(p => p.id === personId);
      if (!person) {
        setError('Person not found');
        return;
      }
      
      // Create the player using the floorball player service
      const newPlayer = await floorballPlayerService.create({ personId });
      
      // Remove the person from available persons list
      setAvailablePersons(prev => prev.filter(person => person.id !== personId));
      
      // Clear any existing timeout to prevent flickering
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      
      // Show success message with person's name
      const message = t('floorball.players.playerCreated', '{{personName}} is now a floorball player!', { 
        personName: person.fullName 
      });
      setSuccessMessage(message);
      
      // Auto-hide success message after 3 seconds
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
      }, 3000);
      setSuccessTimeoutId(timeoutId);
      
      onPlayerCreated(newPlayer);
      // Don't close the modal - let user continue or close manually
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create player');
      console.error('Error creating player:', err);
    } finally {
      setCreating(false);
    }
  };

  // Handle person creation from embedded form
  const handlePersonCreated = (newPerson: Person) => {
    // Add to available persons list
    setAvailablePersons(prev => [newPerson, ...prev]);
    
    // Track newly created persons for highlighting
    setNewlyCreatedPersons(prev => [...prev, newPerson]);
    
    // Switch back to selection mode
    setModalMode('selectPerson');
    
    // Show success message
    const message = t('floorball.players.personCreated', '{{personName}} created successfully! You can now create a player from them.', { 
      personName: newPerson.fullName 
    });
    setSuccessMessage(message);
    
    // Auto-hide success message after 3 seconds
    if (successTimeoutId) {
      clearTimeout(successTimeoutId);
    }
    const timeoutId = setTimeout(() => {
      setSuccessMessage(null);
      setSuccessTimeoutId(null);
    }, 3000);
    setSuccessTimeoutId(timeoutId);
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      {/* Floating Success Toast */}
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      
      <div className={`create-player-modal-content ${modalMode === 'createPerson' ? 'create-person-mode' : 'select-person-mode'}`} onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>
            {modalMode === 'selectPerson' 
              ? t('floorball.players.createFromPerson', 'Create Player from Available Persons')
              : t('floorball.players.createNewPerson', 'Create New Person')
            }
          </h2>
          <button
            className="create-player-modal-close"
            onClick={onClose}
            type="button"
            aria-label="Close modal"
          >
            ×
          </button>
        </div>

        <div className="modal-body">
          {modalMode === 'selectPerson' ? (
            <>
              {/* Search Bar with Create Person Button */}
              <div className="search-container">
                <input
                  type="text"
                  placeholder={t('floorball.players.searchPersons', 'Search available persons...')}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="search-input"
                />
                <button
                  className="create-person-button"
                  onClick={() => setModalMode('createPerson')}
                  type="button"
                >
                  ➕ {t('floorball.players.createNewPerson', 'Create New Person')}
                </button>
              </div>
            </>
          ) : (
            <>
              {/* Back Button for Person Creation Mode */}
              <div className="mode-header">
                <button
                  className="back-button"
                  onClick={() => setModalMode('selectPerson')}
                  type="button"
                >
                  ← {t('common.back', 'Back to Person Selection')}
                </button>
              </div>
              
              {/* Embedded PersonForm */}
              <div className="person-form-container">
                <PersonForm
                  mode="embedded"
                  showTeamAssignment={false}
                  onSuccess={handlePersonCreated}
                  onCancel={() => setModalMode('selectPerson')}
                />
              </div>
            </>
          )}

          {/* Content for Select Person Mode */}
          {modalMode === 'selectPerson' && (
            <>
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
                        t('floorball.players.noPersonsFound', 'No persons found matching your search') :
                        t('floorball.players.noPersonsAvailable', 'No available persons to convert to players. All persons are already players.')
                      }</p>
                    </div>
                  ) : (
                    <>
                      {/* Table Header */}
                      <div className="persons-table-header">
                        <div className="header-birth-date">{t('common.birthDate', 'BIRTH DATE')}</div>
                        <div className="header-registration">{t('common.registration', 'REGISTRATION')}</div>
                        <div className="header-name">{t('common.name', 'NAME')}</div>
                        <div className="header-actions">{t('common.actions', 'ACTIONS')}</div>
                      </div>
                      
                      {/* Persons List */}
                      <div className="persons-list">
                        {filteredPersons.map((person) => (
                          <div 
                            key={person.id} 
                            className={`person-item ${isPersonNewlyCreated(person) ? 'newly-created' : ''}`}
                          >
                            <div className="person-birth-date">
                              {new Date(person.birthDate).toLocaleDateString()}
                            </div>
                            <div className="person-registration">
                              <span className={`registration-indicator ${person.isRegistered ? 'registered' : 'not-registered'}`}>
                                {person.isRegistered ? '✓' : '✗'}
                              </span>
                            </div>
                            <div className="person-name">
                              {person.fullName}
                            </div>
                            <div className="person-actions">
                              <button
                                className="create-player-btn"
                                onClick={() => handleCreatePlayer(person.id)}
                                disabled={creating}
                              >
                                {creating ? 
                                  t('common.creating', 'Creating...') : 
                                  t('floorball.players.createPlayer', 'Create Player')
                                }
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </>
                  )}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default CreatePlayerModal; 