import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { Person } from '../../../../../types/admin/personTypes';
import { PersonRole } from '../../../../../types/admin/personTypes';
import { personApi } from '../../../../../api/admin/personApi';
import PaginationControls from '../PaginationControls/PaginationControls';
import './PersonList.scss';

interface PersonListProps {
  onEditPerson?: (personId: string) => void;
  refreshTrigger?: number; // Used to trigger refresh from parent
}

const PersonList = ({ onEditPerson, refreshTrigger }: PersonListProps) => {
  const { t } = useTranslation();
  const [persons, setPersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingRegistration, setUpdatingRegistration] = useState<string | null>(null);
  const [updatingRole, setUpdatingRole] = useState<string | null>(null);
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  // Search state
  const [searchTerm, setSearchTerm] = useState('');

  const fetchPersons = async () => {
    try {
      setLoading(true);
      const data = await personApi.getAll();
      setPersons(data);
      setError(null);
    } catch (error) {
      console.error('Failed to fetch persons:', error);
      setError(t('admin.persons.errors.fetchFailed', 'Failed to fetch persons'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPersons();
  }, [t, refreshTrigger]); // Added refreshTrigger dependency

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
        await fetchPersons();
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

  const handleRoleChange = async (id: string, newRole: PersonRole) => {
    console.log('Role change requested:', { id, newRole, type: typeof newRole }); // Debug log
    const roleText = t(`admin.persons.roles.${newRole.toLowerCase()}`, newRole);
    const confirmMessage = t('admin.persons.confirmRoleChange', 'Are you sure you want to change this person\'s role to {{role}}?', { role: roleText });
    
    if (window.confirm(confirmMessage)) {
      setUpdatingRole(id);
      try {
        const updatedPerson = await personApi.updateRole(id, newRole);
        console.log('Updated person received:', updatedPerson); // Debug log
        setPersons(persons.map(person => 
          person.id === id ? updatedPerson : person
        ));
        setError(null);
        
        // Show success message
        const successMessage = t('admin.persons.success.roleUpdated', 'Person role updated successfully');
        console.log(successMessage); // You can replace this with a toast notification system
      } catch (error) {
        console.error('Failed to update person role:', error);
        setError(t('admin.persons.errors.updateRoleFailed', 'Failed to update person role'));
      } finally {
        setUpdatingRole(null);
      }
    }
  };

  // Search filtering
  const filteredPersons = persons.filter(person =>
    person.fullName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Pagination calculations (applied to filtered results)
  const totalCount = filteredPersons.length;
  const totalPages = Math.ceil(totalCount / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedPersons = filteredPersons.slice(startIndex, endIndex);

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  };

  // Handle search input change
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setCurrentPage(1); // Reset to first page when searching
  };

  if (loading) {
    return <div className="persons-loading">{t('admin.persons.loading', 'Loading persons...')}</div>;
  }

  if (error) {
    return <div className="persons-error">{error}</div>;
  }

  return (
    <div className="persons-list">
      {/* Search Bar */}
      <div className="persons-search-bar">
        <input
          type="text"
          value={searchTerm}
          onChange={handleSearchChange}
          placeholder={t('admin.persons.searchPlaceholder', 'Search persons by name...') as string}
          className="persons-search-input"
        />
        {searchTerm && (
          <button
            className="search-clear-button"
            onClick={() => setSearchTerm('')}
            title={t('admin.persons.clearSearch', 'Clear search')}
          >
            ✕
          </button>
        )}
      </div>

      {/* Pagination Controls - Top */}
      <PaginationControls
        currentPage={currentPage}
        totalPages={totalPages}
        totalCount={totalCount}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      <table>
        <thead>
          <tr>
            <th>{t('admin.persons.table.name', 'Name')}</th>
            <th>{t('admin.persons.table.birthDate', 'Birth Date')}</th>
            <th>{t('admin.persons.table.email', 'Email')}</th>
            <th>{t('admin.persons.table.registered', 'Registered')}</th>
            <th>{t('admin.persons.table.role', 'Role')}</th>
            <th>{t('admin.persons.table.actions', 'Actions')}</th>
          </tr>
        </thead>
        <tbody>
          {paginatedPersons.map(person => (
            <tr key={person.id}>
              <td>{person.fullName}</td>
              <td>{new Date(person.birthDate).toLocaleDateString()}</td>
              <td>{person.contactInfo?.email || '-'}</td>
              <td>
                <button
                  className={`status-toggle ${person.isRegistered ? 'registered' : 'not-registered'} ${updatingRegistration === person.id ? 'updating' : ''}`}
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
              <td className="role-cell">
                <select
                  className={`role-selector ${updatingRole === person.id ? 'updating' : ''}`}
                  value={person.role}
                  onChange={(e) => handleRoleChange(person.id, e.target.value as PersonRole)}
                  disabled={updatingRole === person.id}
                  title={t('admin.persons.actions.updateRole', 'Update Role')}
                >
                  <option value={PersonRole.User}>
                    {t('admin.persons.roles.user', 'User')}
                  </option>
                  <option value={PersonRole.Admin}>
                    {t('admin.persons.roles.admin', 'Admin')}
                  </option>
                  <option value={PersonRole.SuperAdmin}>
                    {t('admin.persons.roles.superAdmin', 'Super Admin')}
                  </option>
                </select>
                {updatingRole === person.id && (
                  <span className="loading-spinner">⏳</span>
                )}
              </td>
              <td>
                <div className="action-buttons">
                  <button
                    className="edit-button"
                    onClick={() => handleEdit(person.id)}
                  >
                    {t('admin.persons.actions.edit', 'Edit')}
                  </button>
                  <button
                    className="delete-button"
                    onClick={() => handleDelete(person.id)}
                  >
                    {t('admin.persons.actions.delete', 'Delete')}
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

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

      {persons.length > 0 && filteredPersons.length === 0 && (
        <div className="no-search-results">
          {t('admin.persons.noSearchResults', 'No persons found matching "{{searchTerm}}"', { searchTerm })}
        </div>
      )}
    </div>
  );
};

export default PersonList; 