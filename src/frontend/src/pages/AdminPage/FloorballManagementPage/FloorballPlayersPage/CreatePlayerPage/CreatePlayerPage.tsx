import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useLocation } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../../../components/BackButton/BackButton';
import { personApi } from '../../../../../api/admin/personApi';
import { floorballPlayerService } from '../../../../../api/floorball/floorballPlayerService';
import type { Person } from '../../../../../types/admin/personTypes';
import { formatDate } from '../../../../../utils/helpers';
import './CreatePlayerPage.scss';
import SearchField from '../../../../../components/SearchField';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';
import Button from '../../../../../components/Button/Button';
import AddIcon from '../../../../../assets/basicIcons/add.svg';

type SortField = 'birthDate' | 'registration' | 'name';
type SortDirection = 'asc' | 'desc';

const CreatePlayerPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const [availablePersons, setAvailablePersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  
  // Sorting state
  const [sortField, setSortField] = useState<SortField>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  
  // Selection state for multiselect
  const [selectedPersons, setSelectedPersons] = useState<Set<string>>(new Set());
  const [bulkCreating, setBulkCreating] = useState(false);
  const [showBulkConfirmation, setShowBulkConfirmation] = useState(false);

  // Fetch persons and filter out those who are already players
  useEffect(() => {
    const fetchData = async () => {
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
        
        // Check if we have a newly created person from navigation state
        const state = location.state as { newPersonCreated?: Person; successMessage?: string } | null;
        if (state?.newPersonCreated && state?.successMessage) {
          setSuccessMessage(state.successMessage);
          
          // Auto-hide success message after 3 seconds
          const timeoutId = setTimeout(() => {
            setSuccessMessage(null);
            setSuccessTimeoutId(null);
          }, 3000);
          setSuccessTimeoutId(timeoutId);
          
          // Clear navigation state
          navigate(location.pathname, { replace: true, state: null });
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load data');
        console.error('Error fetching data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [location.state, location.pathname, navigate]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  // Handle sorting
  const handleSort = (field: SortField) => {
    if (sortField === field) {
      // Toggle direction if same field
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      // New field, default to ascending
      setSortField(field);
      setSortDirection('asc');
    }
  };

  // Filter and sort available persons
  const filteredAndSortedPersons = availablePersons
    .filter(person =>
      person.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      person.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      person.lastName.toLowerCase().includes(searchTerm.toLowerCase())
    )
    .sort((a, b) => {
      let comparison = 0;
      
      switch (sortField) {
        case 'birthDate': {
          const dateA = new Date(a.birthDate);
          const dateB = new Date(b.birthDate);
          comparison = dateA.getTime() - dateB.getTime();
          break;
        }
        case 'registration':
          // Sort by registration status: registered first (true > false)
          comparison = (b.isRegistered ? 1 : 0) - (a.isRegistered ? 1 : 0);
          break;
        case 'name':
          comparison = a.fullName.localeCompare(b.fullName);
          break;
        default:
          comparison = 0;
      }
      
      return sortDirection === 'asc' ? comparison : -comparison;
    });

  
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
      await floorballPlayerService.create({ personId });
      
      // Remove the person from available persons list
      setAvailablePersons(prev => prev.filter(person => person.id !== personId));
      
      // Clear selection if the created player was selected
      setSelectedPersons(prev => {
        const newSet = new Set(prev);
        newSet.delete(personId);
        return newSet;
      });
      
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
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create player');
      console.error('Error creating player:', err);
    } finally {
      setCreating(false);
    }
  };

  const handleCreateNewPerson = () => {
    navigate('/admin/floorball/players/create-person');
  };

  // Selection management functions
  const togglePersonSelection = (personId: string) => {
    setSelectedPersons(prev => {
      const newSet = new Set(prev);
      if (newSet.has(personId)) {
        newSet.delete(personId);
      } else {
        newSet.add(personId);
      }
      return newSet;
    });
  };

  const selectAllFilteredPersons = () => {
    setSelectedPersons(new Set(filteredAndSortedPersons.map(p => p.id)));
  };

  const clearSelection = () => {
    setSelectedPersons(new Set());
  };

  const handleBulkCreatePlayers = () => {
    if (selectedPersons.size === 0) return;
    setShowBulkConfirmation(true);
  };

  const confirmBulkCreatePlayers = async () => {
    if (selectedPersons.size === 0) return;

    try {
      setBulkCreating(true);
      setError(null);
      setSuccessMessage(null);
      setShowBulkConfirmation(false);
      
      // Clear any existing timeout to prevent flickering
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      
      const selectedPersonsList = availablePersons.filter(p => selectedPersons.has(p.id));
      
      // Create players for all selected persons
      for (const person of selectedPersonsList) {
        await floorballPlayerService.create({ personId: person.id });
      }
      
      // Remove the created players from available persons list
      setAvailablePersons(prev => prev.filter(person => !selectedPersons.has(person.id)));
      
      // Clear selection
      setSelectedPersons(new Set());
      
      // Show success message
      const message = t('floorball.players.bulkPlayersCreated', 
        '{{count}} player(s) created successfully!', 
        { count: selectedPersonsList.length }
      );
      setSuccessMessage(message);
      
      // Auto-hide success message after 3 seconds
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
      }, 3000);
      setSuccessTimeoutId(timeoutId);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create players');
      console.error('Error creating players:', err);
    } finally {
      setBulkCreating(false);
    }
  };

  const cancelBulkCreate = () => {
    setShowBulkConfirmation(false);
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.players.createFromPerson', 'Create Player from Available Persons')}>
        <div className="create-player-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  
  return (
    <PageTemplate title={t('floorball.players.createFromPerson', 'Create Player from Available Persons')}>
      <div className="create-player-container">
        {/* Floating Success Toast */}
        {successMessage && (
          <div className="success-toast">
            <p>{successMessage}</p>
          </div>
        )}

        {/* Back button */}
        <BackButton 
          to="/admin/floorball/players" 
          text={t('common.back', 'Back to Players')} 
        />
        
        {/* Search Bar with Create Person Button */}
        <div className="search-container">
          <SearchField
            value={searchTerm}
            onChange={(val) => {
              setSearchTerm(val);
              setSelectedPersons(new Set());
            }}
            placeholder={t('floorball.players.searchPersons', 'Search available persons...')}
            fullWidth
            rounded="pill"
          />
          <Button
            variant="primary"
            size="md"
            rounded="pill"
            onClick={handleCreateNewPerson}
            iconLeft={AddIcon}
          >
            {t('floorball.players.createNewPerson', 'Create New Person')}
          </Button>
        </div>
        
        {/* Selection Controls */}
        <div className="selection-controls">
          <div className="selection-info">
            
            {filteredAndSortedPersons.length > 0 && (
              <div className="selection-buttons">
                <button
                  type="button"
                  className="control-btn"
                  onClick={selectAllFilteredPersons}
                  disabled={selectedPersons.size === filteredAndSortedPersons.length}
                >
                  {t('common.selectAll', 'Select All')} ({filteredAndSortedPersons.length})
                </button>
                <button
                  type="button"
                  className="control-btn"
                  onClick={clearSelection}
                  disabled={selectedPersons.size === 0}
                >
                  {t('common.clear', 'Clear')}
                </button>
              </div>
            )}
          </div>
          
          {/* Bulk Actions */}
          {selectedPersons.size > 0 && (
            <div className="bulk-actions">
              <button
                type="button"
                className="bulk-create-btn"
                onClick={handleBulkCreatePlayers}
                disabled={bulkCreating}
              >
                {bulkCreating 
                  ? t('floorball.players.actions.bulkCreating', 'Creating players...')
                  : t('floorball.players.actions.bulkCreatePlayers', 'Create {{count}} Player(s)', { count: selectedPersons.size })
                }
              </button>
            </div>
          )}
        </div>

        {/* Error Message */}
        {error && (
          <div className="error-message">
            <p>{error}</p>
          </div>
        )}

        {/* Persons List */}
        <div className="persons-container">
          {filteredAndSortedPersons.length === 0 ? (
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
                <div className="header-select">
                  <input
                    type="checkbox"
                    checked={filteredAndSortedPersons.length > 0 && filteredAndSortedPersons.every(person => selectedPersons.has(person.id))}
                    onChange={(e) => {
                      if (e.target.checked) {
                        selectAllFilteredPersons();
                      } else {
                        clearSelection();
                      }
                    }}
                    title={t('floorball.players.selectAllVisible', 'Select all visible persons')}
                  />
                </div>
                <div className="header-birth-date" onClick={() => handleSort('birthDate')} style={{ cursor: 'pointer' }}>
                  {t('common.birthDate', 'BIRTH DATE')}
                  {sortField === 'birthDate' && (
                    <span style={{ marginLeft: '5px' }}>
                      {sortDirection === 'asc' ? '↑' : '↓'}
                    </span>
                  )}
                </div>
                <div className="header-registration" onClick={() => handleSort('registration')} style={{ cursor: 'pointer' }}>
                  {t('common.registration', 'REGISTRATION')}
                  {sortField === 'registration' && (
                    <span style={{ marginLeft: '5px' }}>
                      {sortDirection === 'asc' ? '↑' : '↓'}
                    </span>
                  )}
                </div>
                <div className="header-name" onClick={() => handleSort('name')} style={{ cursor: 'pointer' }}>
                  {t('common.name', 'NAME')}
                  {sortField === 'name' && (
                    <span style={{ marginLeft: '5px' }}>
                      {sortDirection === 'asc' ? '(Asc.)' : '(Desc.)'}
                    </span>
                  )}
                </div>
                <div className="header-actions">{t('common.actions', 'ACTIONS')}</div>
              </div>
              
              {/* Persons List */}
              <div className="persons-list">
                {filteredAndSortedPersons.map((person) => (
                  <div 
                    key={person.id} 
                    className={`create-player-person-item ${selectedPersons.has(person.id) ? 'selected' : ''}`}
                  >
                    <div className="person-select-column">
                      <input
                        type="checkbox"
                        checked={selectedPersons.has(person.id)}
                        onChange={() => togglePersonSelection(person.id)}
                        onClick={(e) => e.stopPropagation()}
                      />
                    </div>
                    <div 
                      className="create-player-person-birth-date clickable-cell"
                      onClick={() => togglePersonSelection(person.id)}
                    >
                      {formatDate(person.birthDate)}
                    </div>
                    <div 
                      className="create-player-person-registration clickable-cell"
                      onClick={() => togglePersonSelection(person.id)}
                    >
                      <span 
                        className={`registration-badge ${person.isRegistered ? 'active' : 'inactive'}`}
                        aria-label={person.isRegistered ? t('common.registered', 'Registered') : t('common.notRegistered', 'Not Registered')}
                        title={person.isRegistered ? t('common.registered', 'Registered') : t('common.notRegistered', 'Not Registered')}
                      >
                        <img
                          src={person.isRegistered ? CheckIcon : CloseIcon}
                          alt={person.isRegistered ? t('common.registered', 'Registered') : t('common.notRegistered', 'Not Registered')}
                          className="registration-icon"
                        />
                      </span>
                    </div>
                    <div 
                      className="create-player-person-name clickable-cell"
                      onClick={() => togglePersonSelection(person.id)}
                    >
                      {person.fullName}
                    </div>
                    <div className="create-player-person-actions">
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
        
        {/* Bulk Create Confirmation Modal */}
        {showBulkConfirmation && (
          <div className="modal-overlay" onClick={cancelBulkCreate}>
            <div className="bulk-create-modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>{t('floorball.players.confirmBulkCreate.title', 'Confirm Bulk Player Creation')}</h2>
                <button
                  className="modal-close"
                  onClick={cancelBulkCreate}
                  type="button"
                  aria-label="Close modal"
                  disabled={bulkCreating}
                >
                  ×
                </button>
              </div>
              
              <div className="modal-body">
                <div className="warning-icon">
                  ⚠️
                </div>
                <div className="confirm-message">
                  <p>
                    {t('floorball.players.confirmBulkCreate.message', 
                      'Are you sure you want to create {{count}} player(s) from the selected persons?', 
                      { count: selectedPersons.size }
                    )}
                  </p>
                  <p className="info-text">
                    {t('floorball.players.confirmBulkCreate.info', 'This will convert the selected persons into floorball players.')}
                  </p>
                </div>
              </div>
              
              <div className="modal-footer">
                <button
                  type="button"
                  onClick={cancelBulkCreate}
                  className="cancel-button"
                  disabled={bulkCreating}
                >
                  {t('common.cancel', 'Cancel')}
                </button>
                <button
                  type="button"
                  onClick={confirmBulkCreatePlayers}
                  className="create-button"
                  disabled={bulkCreating}
                >
                  {bulkCreating 
                    ? t('floorball.players.actions.bulkCreating', 'Creating players...')
                    : t('floorball.players.actions.confirmBulkCreate', 'Create All Players')
                  }
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </PageTemplate>
  );
};

export default CreatePlayerPage;
