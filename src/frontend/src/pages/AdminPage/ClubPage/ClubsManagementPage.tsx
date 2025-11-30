import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../components/BackButton/BackButton';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { clubService, type Club } from '../../../api/common/clubService';
import './ClubsManagementPage.scss';
import Pagination from '../../../components/Pagination/Pagination';

type SortKey = 'name' | 'city' | 'country' | 'foundingDate';
type SortDir = 'asc' | 'desc';

function ClubsManagementPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState<string>('');
  const [sortKey, setSortKey] = useState<SortKey>('name');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [isSearching, setIsSearching] = useState<boolean>(false);

  const fetchClubs = useCallback(async (page: number, size: number) => {
    setLoading(true);
    setError(null);
    try {
      const resp = await clubService.getPaged(page, size);
      setClubs(resp.data);
      setCurrentPage(resp.pagination.currentPage);
      setTotalPages(resp.pagination.totalPages);
      setTotalCount(resp.pagination.totalCount);
      setPageSize(resp.pagination.pageSize);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  }, []);

  const searchClubs = useCallback(async (searchTerm: string) => {
    if (!searchTerm.trim() || searchTerm.trim().length < 2) {
      return;
    }

    setLoading(true);
    setError(null);
    setIsSearching(true);
    try {
      const results = await clubService.searchByName(searchTerm.trim());
      setClubs(results);
      setTotalCount(results.length);
      setCurrentPage(1);
      setTotalPages(1);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
      setClubs([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, []);

  // Debounced search effect
  useEffect(() => {
    const handler = setTimeout(() => {
      if (search.trim().length >= 2) {
        searchClubs(search);
      } else if (search.trim().length === 0) {
        // Reset to paginated view when search is cleared
        setIsSearching(false);
        fetchClubs(1, pageSize);
      }
    }, 300);

    return () => {
      clearTimeout(handler);
    };
  }, [search, searchClubs, pageSize, fetchClubs]);

  // Initial load on mount and when pageSize changes (only if not searching)
  useEffect(() => {
    if (!isSearching) {
      fetchClubs(1, pageSize);
    }
  }, [pageSize, isSearching, fetchClubs]);

  const sortedClubs = useMemo(() => {
    const compare = (a: Club, b: Club) => {
      const dir = sortDir === 'asc' ? 1 : -1;
      switch (sortKey) {
        case 'name':
          return a.name.localeCompare(b.name) * dir;
        case 'city':
          return a.city.localeCompare(b.city) * dir;
        case 'country':
          return a.country.localeCompare(b.country) * dir;
        case 'foundingDate': {
          const da = new Date(a.foundingDate).getTime();
          const db = new Date(b.foundingDate).getTime();
          return (da - db) * (sortDir === 'asc' ? 1 : -1);
        }
      }
    };
    return [...clubs].sort(compare);
  }, [clubs, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) {
      setSortDir((prev) => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDir('asc');
    }
  };

  const formatDmy = (iso: string) => {
    const dt = new Date(iso);
    const dd = String(dt.getUTCDate()).padStart(2, '0');
    const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
    const yyyy = dt.getUTCFullYear();
    return `${dd}-${mm}-${yyyy}`;
  };

  return (
    <AdminPageTemplate title={t('clubs.manage.title', 'Manage Clubs')}>
      <div className="clubs-page">
        <BackButton to="/admin" text={t('common.back', 'Back')} />

        <div className="clubs-header">
          <div className="left">
            <h2 className="page-title-compact font-title">
              {t('clubs.manage.title', 'Manage Clubs')}
            </h2>
            <p className="count">
              {isSearching 
                ? t('clubs.searchResults', 'Search Results'): t('clubs.total', 'Total')}: {totalCount}
            </p>
          </div>
          <div className="right">
            <button
              className="btn create-club-button"
              onClick={() => navigate('/admin/clubs/create')}
            >
              + {t('clubs.createNew', 'Create New Club')}
            </button>
          </div>
        </div>

        <div className="clubs-toolbar">
          <input
            className="search-input"
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('clubs.searchPlaceholder', 'Search clubs by name...')}
            aria-label={t('clubs.searchAria', 'Search clubs')}
          />
        </div>

        <ErrorPopup message={error} />

        <div className="clubs-table-wrapper">
          <table className="clubs-table">
            <thead>
              <tr>
                <th>
                  <button className={`table-sort ${sortKey === 'name' ? 'active' : ''}`} onClick={() => toggleSort('name')}>
                    {t('clubs.table.name', 'Club Name')}
                    {sortKey === 'name' && <span className="arrow">{sortDir === 'asc' ? '▲' : '▼'}</span>}
                  </button>
                </th>
                <th>
                  <button className={`table-sort ${sortKey === 'city' ? 'active' : ''}`} onClick={() => toggleSort('city')}>
                    {t('clubs.table.city', 'City')}
                    {sortKey === 'city' && <span className="arrow">{sortDir === 'asc' ? '▲' : '▼'}</span>}
                  </button>
                </th>
                <th>
                  <button className={`table-sort ${sortKey === 'country' ? 'active' : ''}`} onClick={() => toggleSort('country')}>
                    {t('clubs.table.country', 'Country')}
                    {sortKey === 'country' && <span className="arrow">{sortDir === 'asc' ? '▲' : '▼'}</span>}
                  </button>
                </th>
                <th>
                  <button className={`table-sort ${sortKey === 'foundingDate' ? 'active' : ''}`} onClick={() => toggleSort('foundingDate')}>
                    {t('clubs.table.foundingDate', 'Founding Date')}
                    {sortKey === 'foundingDate' && <span className="arrow">{sortDir === 'asc' ? '▲' : '▼'}</span>}
                  </button>
                </th>
                <th className="actions-col">{t('clubs.table.actions', 'Actions')}</th>
              </tr>
            </thead>
            <tbody>
              {!loading && sortedClubs.length === 0 && (
                <tr>
                  <td colSpan={5} className="empty-row">
                    {isSearching 
                      ? t('clubs.noSearchResults', 'No clubs found matching your search')
                      : t('clubs.empty', 'No clubs found')}
                  </td>
                </tr>
              )}

              {loading && (
                <tr>
                  <td colSpan={5} className="loading-row">
                    {t('common.loading', 'Loading...')}
                  </td>
                </tr>
              )}

              {!loading &&
                sortedClubs.map((club) => (
                  <tr
                    key={club.id}
                    className="clickable-row"
                    onClick={() => navigate(`/admin/clubs/${club.id}`)}
                  >
                    <td data-label={t('clubs.table.name', 'Club Name')}>{club.name}</td>
                    <td data-label={t('clubs.table.city', 'City')}>{club.city}</td>
                    <td data-label={t('clubs.table.country', 'Country')}>{club.country}</td>
                    <td data-label={t('clubs.table.foundingDate', 'Founding Date')}>{formatDmy(club.foundingDate)}</td>
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
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>

        {!isSearching && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={(page) => fetchClubs(page, pageSize)}
            onPageSizeChange={(size) => {
              setCurrentPage(1);
              fetchClubs(1, size);
            }}
            pageSizeOptions={[25, 50, 100]}
            className=""
          />
        )}
      </div>
    </AdminPageTemplate>
  );
}

export default ClubsManagementPage;


