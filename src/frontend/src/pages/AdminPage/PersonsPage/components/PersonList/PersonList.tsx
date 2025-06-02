import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { Person } from '../../../../../types/admin/personTypes';
import { personApi } from '../../../../../api/admin/personApi';
import './PersonList.scss';

const PersonList = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [persons, setPersons] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
                <span className={`status-badge ${person.isRegistered ? 'registered' : 'not-registered'}`}>
                  {person.isRegistered 
                    ? t('admin.persons.status.registered', 'Yes')
                    : t('admin.persons.status.notRegistered', 'No')}
                </span>
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