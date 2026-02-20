import { useState, useEffect, useLayoutEffect, useCallback, useRef, memo, useImperativeHandle, forwardRef } from 'react';
import { useTranslation } from 'react-i18next';
import type { Person, PaginatedApiResponse } from '../../../../../types/admin/personTypes';
import { personApi } from '../../../../../api/admin/personApi';
import PaginationControls from '../PaginationControls/PaginationControls';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../../styles/AdminTable.scss';
import './PersonList.scss';

interface PersonListProps {
  onEditPerson?: (personId: string) => void;
  refreshTrigger?: number; // Used to trigger refresh from parent
}

interface SearchBarProps {
  onSearchChange: (value: string) => void;
  placeholder: string;
}

export interface SearchBarRef {
  clear: () => void;
  focus: () => void;
  getValue: () => string;
}

// Memoized search bar component with internal state - only re-renders when callbacks change
const SearchBar = memo(forwardRef<SearchBarRef, SearchBarProps>(({ onSearchChange, placeholder }, ref) => {
  const [internalValue, setInternalValue] = useState('');
  const inputRef = useRef<HTMLInputElement | null>(null);

  // Expose methods to parent via ref
  useImperativeHandle(ref, () => ({
    clear: () => {
      setInternalValue('');
      inputRef.current?.focus();
    },
    focus: () => {
      inputRef.current?.focus();
    },
    getValue: () => internalValue,
  }));

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setInternalValue(value);
    onSearchChange(value);
  };

  const handleClear = () => {
    setInternalValue('');
    onSearchChange('');
    inputRef.current?.focus();
  };

  return (
    <div className="persons-search-bar">
      <input
        ref={inputRef}
        type="text"
        value={internalValue}
        onChange={handleChange}
        placeholder={placeholder}
        className="persons-search-input"
      />
      {internalValue && (
        <button
          className="search-clear-button"
          onClick={handleClear}
          title="Clear search"
        >
          ✕
        </button>
      )}
    </div>
  );
}));

SearchBar.displayName = 'SearchBar';

const PersonList = ({ onEditPerson, refreshTrigger }: PersonListProps) => {
  const { t } = useTranslation();
  const [persons, setPersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [updatingRegistration, setUpdatingRegistration] = useState<string | null>(null);
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  
  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const debounceTimerRef = useRef<number | null>(null);
  const previousSearchTermRef = useRef('');
  const searchBarRef = useRef<SearchBarRef>(null);
  const shouldRestoreFocusRef = useRef(false);

  // Selection state for multiselect
  const [selectedPersons, setSelectedPersons] = useState<Set<string>>(new Set());
  const [bulkDeleting, setBulkDeleting] = useState(false);

  // Debounce search term - only update if it actually changed
  useEffect(() => {
    // Clear any existing timer
    if (debounceTimerRef.current !== null) {
      clearTimeout(debounceTimerRef.current);
    }

    // Only update if search term actually changed (skip if same as previous)
    if (searchTerm !== previousSearchTermRef.current) {
      debounceTimerRef.current = window.setTimeout(() => {
        // Only search if there are at least 2 characters, otherwise clear search
        if (searchTerm.trim().length >= 2) {
          setDebouncedSearchTerm(searchTerm);
        } else {
          setDebouncedSearchTerm('');
        }
        previousSearchTermRef.current = searchTerm;
      }, 300); // 300ms delay
    }

    return () => {
      if (debounceTimerRef.current !== null) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, [searchTerm]);

  const fetchPersons = useCallback(async (isInitialLoad = false) => {
    // Check if search input has focus before fetching
    // We check if the active element is an input and if searchBarRef exists
    const activeElement = document.activeElement;
    const hadFocus = activeElement?.tagName === 'INPUT' && searchBarRef.current !== null;
    shouldRestoreFocusRef.current = hadFocus;
    
    try {
      if (isInitialLoad) {
        setLoading(true);
      } else {
        setPaginationLoading(true);
      }
      
      if (debouncedSearchTerm.trim()) {
        // Use search API when there's a search term (server-side pagination)
        const response: PaginatedApiResponse<Person> = await personApi.search(
          debouncedSearchTerm.trim(),
          currentPage,
          pageSize
        );
        
        setPersons(response.data);
        setTotalCount(response.pagination.totalCount);
        setTotalPages(response.pagination.totalPages);
      } else {
        // Use getAll when there's no search term (server-side pagination)
        const response: PaginatedApiResponse<Person> = await personApi.getAll(currentPage, pageSize);
        setPersons(response.data);
        setTotalCount(response.pagination.totalCount);
        setTotalPages(response.pagination.totalPages);
      }
      
      setError(null);
    } catch (error) {
      console.error('Failed to fetch persons:', error);
      setError(t('admin.persons.errors.fetchFailed', 'Failed to fetch persons'));
      setPersons([]);
      setTotalCount(0);
      setTotalPages(1);
    } finally {
      if (isInitialLoad) {
        setLoading(false);
      }
      setPaginationLoading(false);
    }
  }, [debouncedSearchTerm, currentPage, pageSize, t]);

  // Restore focus after loading completes if search input had focus
  useLayoutEffect(() => {
    if (!loading && shouldRestoreFocusRef.current && searchBarRef.current) {
      searchBarRef.current.focus();
      shouldRestoreFocusRef.current = false;
    }
  }, [loading]);

  const [isInitialLoad, setIsInitialLoad] = useState(true);

  useEffect(() => {
    const run = async () => {
      await fetchPersons(isInitialLoad);
      if (isInitialLoad) {
        setIsInitialLoad(false);
      }
    };

    run();
  }, [fetchPersons, refreshTrigger, isInitialLoad]);

  const formatBirthDate = (dateString: string | null | undefined): string => {
    if (!dateString) return '-';
    const parsed = new Date(dateString);
    if (Number.isNaN(parsed.getTime())) return '-';
    const day = parsed.getDate().toString().padStart(2, '0');
    const month = (parsed.getMonth() + 1).toString().padStart(2, '0');
    const year = parsed.getFullYear();
    return `${day}-${month}-${year}`;
  };

  const handleEdit = (id: string) => {
    if (onEditPerson) {
      onEditPerson(id);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('admin.persons.actions.confirmDelete', 'Are you sure you want to delete this person?'))) {
      try {
        await personApi.delete(id);
        // Refresh the list to get updated data
        await fetchPersons(false);
        // Clear selection if the deleted person was selected
        setSelectedPersons(prev => {
          const newSet = new Set(prev);
          newSet.delete(id);
          return newSet;
        });
      } catch (error) {
        console.error('Failed to delete person:', error);
        setError(t('admin.persons.errors.deleteFailed', 'Failed to delete person'));
      }
    }
  };

  const handleToggleRegistration = async (id: string, currentStatus: boolean) => {
    const confirmMessage = currentStatus 
      ? t('admin.persons.actions.confirmUnregister', 'Are you sure you want to unregister this person?')
      : t('admin.persons.actions.confirmRegister', 'Are you sure you want to register this person?');
    
    if (window.confirm(confirmMessage)) {
      setUpdatingRegistration(id);
      try {
        const updatedPerson = await personApi.updateRegistration(id, !currentStatus);
        setPersons(persons.map(person => 
          person.id === id ? updatedPerson : person
        ));
        setError(null);
        
        // Show success message
        const successMessage = !currentStatus
          ? t('admin.persons.success.registered', 'Person registered successfully')
          : t('admin.persons.success.unregistered', 'Person unregistered successfully');
        console.log(successMessage); // You can replace this with a toast notification system
      } catch (error) {
        console.error('Failed to update registration status:', error);
        setError(t('admin.persons.errors.updateRegistrationFailed', 'Failed to update registration status'));
      } finally {
        setUpdatingRegistration(null);
      }
    }
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
    setSelectedPersons(new Set(persons.map(p => p.id)));
  };

  const clearSelection = () => {
    setSelectedPersons(new Set());
  };

  const handleBulkDelete = async () => {
    if (selectedPersons.size === 0) return;
    
    const confirmMessage = t('admin.persons.actions.confirmBulkDelete', 
      'Are you sure you want to delete {{count}} selected person(s)? This action cannot be undone.', 
      { count: selectedPersons.size }
    );
    
    if (window.confirm(confirmMessage)) {
      setBulkDeleting(true);
      try {
        // Delete each selected person
        for (const personId of selectedPersons) {
          await personApi.delete(personId);
        }
        
        // Refresh the list and clear selection
        await fetchPersons(false);
        setSelectedPersons(new Set());
        
        // Show success message
        const successMessage = t('admin.persons.success.bulkDeleted', 
          '{{count}} person(s) deleted successfully', 
          { count: selectedPersons.size }
        );
        console.log(successMessage);
      } catch (error) {
        console.error('Failed to delete selected persons:', error);
        setError(t('admin.persons.errors.bulkDeleteFailed', 'Failed to delete selected persons'));
      } finally {
        setBulkDeleting(false);
      }
    }
  };

  // Backend already paginates both search and non-search responses
  const paginatedPersons = persons;

  // Handle page change
  const handlePageChange = useCallback((page: number) => {
    setCurrentPage(page);
  }, []);

  // Handle page size change
  const handlePageSizeChange = useCallback((newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  }, []);

  // Handle search input change - receives value directly, not event
  const handleSearchChange = useCallback((value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
    setSelectedPersons(new Set()); // Clear selection when searching
    
    // If clearing search or less than 2 characters, immediately clear debounced term
    if (!value.trim() || value.trim().length < 2) {
      setDebouncedSearchTerm('');
      previousSearchTermRef.current = value;
    }
  }, []);

  return (
    <div className="persons-list">
      {/* Search Bar - Always rendered to preserve state */}
      <SearchBar
        ref={searchBarRef}
        onSearchChange={handleSearchChange}
        placeholder={t('admin.persons.searchPlaceholder', 'Search persons by name...') as string}
      />

      {loading && (
        <div className="persons-loading">{t('admin.persons.loading', 'Loading persons...')}</div>
      )}

      {error && (
        <div className="persons-error">{error}</div>
      )}

      {!loading && !error && (
        <>
          {/* Bulk Actions Bar */}
          <BulkActionsBar
            selectedCount={selectedPersons.size}
            totalCount={persons.length}
            onSelectAll={selectAllFilteredPersons}
            onClearSelection={clearSelection}
            actions={[
              {
                label: bulkDeleting
                  ? t('admin.persons.actions.deleting', 'Deleting...')
                  : t('common.delete', 'Delete'),
                onClick: handleBulkDelete,
                variant: 'danger',
                disabled: bulkDeleting,
              },
            ]}
          />

          {/* Pagination Controls - Top */}
          <PaginationControls
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
          />

          <div className="admin-table__wrapper persons-table-area">
            <table className="admin-table">
              <thead>
          <tr>
            <th className="admin-table__checkbox-col">
              <input
                type="checkbox"
                checked={paginatedPersons.length > 0 && paginatedPersons.every(person => selectedPersons.has(person.id))}
                onChange={(e) => {
                  if (e.target.checked) {
                    const newSelection = new Set(selectedPersons);
                    paginatedPersons.forEach(person => newSelection.add(person.id));
                    setSelectedPersons(newSelection);
                  } else {
                    const newSelection = new Set(selectedPersons);
                    paginatedPersons.forEach(person => newSelection.delete(person.id));
                    setSelectedPersons(newSelection);
                  }
                }}
                title={t('admin.persons.selectAllOnPage', 'Select all on this page')}
              />
            </th>
            <th>{t('admin.persons.table.name', 'Name')}</th>
            <th>{t('admin.persons.table.birthDate', 'Birth Date')}</th>
            <th>{t('admin.persons.table.email', 'Email')}</th>
            <th>{t('admin.persons.table.registered', 'Registered')}</th>
            <th className="admin-table__actions-col">{t('admin.persons.table.actions', 'Actions')}</th>
          </tr>
              </thead>
              <tbody>
              {paginatedPersons.map(person => (
            <tr 
              key={person.id}
              className={selectedPersons.has(person.id) ? 'admin-table__row--selected' : ''}
            >
              <td className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={selectedPersons.has(person.id)}
                  onChange={() => togglePersonSelection(person.id)}
                  onClick={(e) => e.stopPropagation()}
                />
              </td>
              <td onClick={() => togglePersonSelection(person.id)} className="clickable-cell">
                {person.fullName}
              </td>
              <td onClick={() => togglePersonSelection(person.id)} className="clickable-cell">
                {formatBirthDate(person.birthDate)}
              </td>
              <td onClick={() => togglePersonSelection(person.id)} className="clickable-cell">
                {person.contactInfo?.email || '-'}
              </td>
              <td>
                <button
                  className={`admin-table__toggle-btn ${person.isRegistered ? 'admin-table__toggle-btn--on' : 'admin-table__toggle-btn--off'}`}
                  onClick={() => handleToggleRegistration(person.id, person.isRegistered)}
                  disabled={updatingRegistration === person.id}
                  title={t('admin.persons.actions.toggleRegistration', 'Click to toggle registration status')}
                >
                  {updatingRegistration === person.id ? (
                    <span className="loading-spinner">⏳</span>
                  ) : (
                    <>
                      <span className="status-icon">
                        {person.isRegistered ? '✓' : '✗'}
                      </span>
                      <span className="status-text">
                        {person.isRegistered 
                          ? t('admin.persons.status.registered', 'Yes')
                          : t('admin.persons.status.notRegistered', 'No')}
                      </span>
                    </>
                  )}
                </button>
              </td>
              <td className="admin-table__actions-col">
                <ActionsDropdown
                  ariaLabel={t('admin.persons.actions.menu', 'Person actions menu')}
                  actions={[
                    { label: t('admin.persons.actions.edit', 'Edit'), onClick: () => handleEdit(person.id) },
                    { label: t('admin.persons.actions.delete', 'Delete'), onClick: () => handleDelete(person.id), variant: 'danger' },
                  ]}
                />
              </td>
              </tr>
              ))}
              </tbody>
            </table>
            {paginationLoading && (
              <div className="pagination-loading-overlay">
                <div className="loading-spinner-small" />
              </div>
            )}
          </div>

          {/* Pagination Controls - Bottom */}
          <PaginationControls
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
          />

          {persons.length === 0 && (
        <div className="no-data">
          {t('admin.persons.noData', 'No persons found')}
        </div>
      )}

          {debouncedSearchTerm && persons.length === 0 && (
            <div className="no-search-results">
              {t('admin.persons.noSearchResults', 'No persons found matching "{{searchTerm}}"', { searchTerm: debouncedSearchTerm })}
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default PersonList; 