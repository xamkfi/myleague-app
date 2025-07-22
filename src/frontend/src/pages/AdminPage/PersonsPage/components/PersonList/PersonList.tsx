import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { Person } from '../../../../../types/admin/personTypes';
import { PersonRole } from '../../../../../types/admin/personTypes';
import { personApi } from '../../../../../api/admin/personApi';
import './PersonList.scss';

const PersonList = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [persons, setPersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingRegistration, setUpdatingRegistration] = useState<string | null>(null);
  const [updatingRole, setUpdatingRole] = useState<string | null>(null);

  useEffect(() => {
    const fetchPersons = async () => {
      try {
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

    fetchPersons();
  }, [t]);

  const handleEdit = (id: string) => {
    navigate(`/admin/persons/${id}/edit`);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('admin.persons.confirmDelete', 'Are you sure you want to delete this person?'))) {
      try {
        await personApi.delete(id);
        setPersons(persons.filter(person => person.id !== id));
      } catch (error) {
        console.error('Failed to delete person:', error);
        setError(t('admin.persons.errors.deleteFailed', 'Failed to delete person'));
      }
    }
  };

  const handleToggleRegistration = async (id: string, currentStatus: boolean) => {
    const confirmMessage = currentStatus 
      ? t('admin.persons.confirmUnregister', 'Are you sure you want to unregister this person?')
      : t('admin.persons.confirmRegister', 'Are you sure you want to register this person?');
    
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

  if (loading) {
    return <div className="persons-loading">{t('admin.persons.loading', 'Loading persons...')}</div>;
  }

  if (error) {
    return <div className="persons-error">{error}</div>;
  }

  return (
    <div className="persons-list">
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
          {persons.map(person => (
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
      {persons.length === 0 && (
        <div className="no-data">
          {t('admin.persons.noData', 'No persons found')}
        </div>
      )}
    </div>
  );
};

export default PersonList; 