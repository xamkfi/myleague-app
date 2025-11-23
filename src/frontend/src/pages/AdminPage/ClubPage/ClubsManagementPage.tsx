import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../components/BackButton/BackButton';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { clubService, type Club } from '../../../api/common/clubService';
import './ClubsManagementPage.scss';

function ClubsManagementPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState<string>('');

  const fetchClubs = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await clubService.getAll();
      setClubs(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchClubs();
  }, []);

  const handleDelete = async (id: string) => {
    const confirmed = window.confirm(
      t('clubs.confirmDelete', 'Are you sure you want to delete this club? This action cannot be undone.')
    );
    if (!confirmed) return;
    try {
      await clubService.remove(id);
      await fetchClubs();
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    }
  };

  const filteredClubs = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return clubs;
    return clubs.filter((c) =>
      [c.name, c.city, c.country].some((v) => (v || '').toLowerCase().includes(term))
    );
  }, [clubs, search]);

  return (
    <AdminPageTemplate title={t('clubs.manage.title', 'Manage Clubs')}>
      <div className="clubs-page">
        <BackButton to="/admin" text={t('common.back', 'Back')} />

        <div className="clubs-header">
          <div className="left">
            <h2>{t('clubs.manage.title', 'Manage Clubs')}</h2>
            <p className="count">
              {t('clubs.total', 'Total')}: {clubs.length}
            </p>
          </div>
          <div className="right">
            <button
              className="btn btn-primary"
              onClick={() => navigate('/admin/clubs/create')}
            >
              {t('clubs.createNew', 'Create New Club')}
            </button>
          </div>
        </div>

        <div className="clubs-toolbar">
          <input
            className="search-input"
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('clubs.searchPlaceholder', 'Search clubs by name or city...')}
            aria-label={t('clubs.searchAria', 'Search clubs')}
          />
        </div>

        <ErrorPopup message={error} />

        <div className="clubs-table-wrapper">
          <table className="clubs-table">
            <thead>
              <tr>
                <th>{t('clubs.table.name', 'Club Name')}</th>
                <th>{t('clubs.table.city', 'City')}</th>
                <th>{t('clubs.table.country', 'Country')}</th>
                <th className="actions-col">{t('clubs.table.actions', 'Actions')}</th>
              </tr>
            </thead>
            <tbody>
              {!loading && filteredClubs.length === 0 && (
                <tr>
                  <td colSpan={4} className="empty-row">
                    {t('clubs.empty', 'No clubs found')}
                  </td>
                </tr>
              )}

              {loading && (
                <tr>
                  <td colSpan={4} className="loading-row">
                    {t('common.loading', 'Loading...')}
                  </td>
                </tr>
              )}

              {!loading &&
                filteredClubs.map((club) => (
                  <tr
                    key={club.id}
                    className="clickable-row"
                    onClick={() => navigate(`/admin/clubs/${club.id}`)}
                  >
                    <td data-label={t('clubs.table.name', 'Club Name')}>{club.name}</td>
                    <td data-label={t('clubs.table.city', 'City')}>{club.city}</td>
                    <td data-label={t('clubs.table.country', 'Country')}>{club.country}</td>
                    <td
                      className="actions"
                      onClick={(e) => e.stopPropagation()}
                      aria-label={t('clubs.table.actions', 'Actions')}
                    >
                      <button
                        className="btn btn-secondary"
                        onClick={() => navigate(`/admin/clubs/${club.id}/edit`)}
                      >
                        {t('common.edit', 'Edit')}
                      </button>
                      <button
                        className="btn btn-danger"
                        onClick={() => handleDelete(club.id)}
                      >
                        {t('common.delete', 'Delete')}
                      </button>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </div>
    </AdminPageTemplate>
  );
}

export default ClubsManagementPage;


