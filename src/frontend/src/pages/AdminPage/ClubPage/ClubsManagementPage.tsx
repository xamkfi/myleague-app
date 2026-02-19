import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import ActionsDropdown from '../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../components/BulkActionsBar/BulkActionsBar';
import AddIcon from '../../../assets/basicIcons/add.svg';
import { clubService, type Club } from '../../../api/common/clubService';
import Pagination from '../../../components/Pagination/Pagination';
import '../../../styles/AdminTable.scss';
import './ClubsManagementPage.scss';

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
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

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

  useEffect(() => {
    const handler = setTimeout(() => {
      if (search.trim().length >= 2) {
        searchClubs(search);
      } else if (search.trim().length === 0) {
        setIsSearching(false);
        fetchClubs(1, pageSize);
      }
    }, 300);
    return () => clearTimeout(handler);
  }, [search, searchClubs, pageSize, fetchClubs]);

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
          return (a.name ?? '').localeCompare(b.name ?? '') * dir;
        case 'city':
          return (a.city ?? '').localeCompare(b.city ?? '') * dir;
        case 'country':
          return (a.country ?? '').localeCompare(b.country ?? '') * dir;
        case 'foundingDate': {
          const da = a.foundingDate ? new Date(a.foundingDate).getTime() : 0;
          const db = b.foundingDate ? new Date(b.foundingDate).getTime() : 0;
          return (da - db) * dir;
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

  const formatDmy = (iso: string | null | undefined) => {
    if (!iso) return '-';
    const dt = new Date(iso);
    if (Number.isNaN(dt.getTime()) || dt.getUTCFullYear() <= 1) return '-';
    const dd = String(dt.getUTCDate()).padStart(2, '0');
    const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
    const yyyy = dt.getUTCFullYear();
    return `${dd}-${mm}-${yyyy}`;
  };

  const handleToggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSelectAll = () => setSelectedIds(new Set(sortedClubs.map((c) => c.id)));
  const handleClearSelection = () => setSelectedIds(new Set());

  const handleBulkDelete = async () => {
    for (const id of selectedIds) {
      try {
        await clubService.remove(id);
        setClubs((prev) => prev.filter((c) => c.id !== id));
      } catch (err) {
        console.error('Failed to delete club', err);
      }
    }
    setSelectedIds(new Set());
  };

  const allSelected = sortedClubs.length > 0 && sortedClubs.every((c) => selectedIds.has(c.id));
  const sortIcon = (key: SortKey) => sortKey === key ? (sortDir === 'asc' ? ' ▲' : ' ▼') : '';

  return (
    <AdminPageTemplate title={t('clubs.manage.title', 'Manage Clubs')}>
      <div className="clubs-page">

        <div className="clubs-header">
          <div className="left">
            <h2 className="page-title-compact font-title">
              {t('clubs.manage.title', 'Manage Clubs')}
            </h2>
            <p className="count">
              {isSearching
                ? t('clubs.searchResults', 'Search Results') : t('clubs.total', 'Total')}: {totalCount}
            </p>
          </div>
          <div className="right">
            <Button iconLeft={AddIcon} rounded="pill" onClick={() => navigate('/admin/clubs/create')}>
              {t('clubs.createNew', 'Create New Club')}
            </Button>
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

        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={sortedClubs.length}
          onSelectAll={handleSelectAll}
          onClearSelection={handleClearSelection}
          actions={[
            { label: t('common.delete', 'Delete'), onClick: handleBulkDelete, variant: 'danger' },
          ]}
        />

        <div className="admin-table__wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th className="admin-table__checkbox-col">
                  <input type="checkbox" checked={allSelected} onChange={() => (allSelected ? handleClearSelection() : handleSelectAll())} />
                </th>
                <th className="admin-table__sortable" onClick={() => toggleSort('name')}>
                  {t('clubs.table.name', 'Club Name')}{sortIcon('name')}
                </th>
                <th className="admin-table__sortable" onClick={() => toggleSort('city')}>
                  {t('clubs.table.city', 'City')}{sortIcon('city')}
                </th>
                <th className="admin-table__sortable" onClick={() => toggleSort('country')}>
                  {t('clubs.table.country', 'Country')}{sortIcon('country')}
                </th>
                <th className="admin-table__sortable" onClick={() => toggleSort('foundingDate')}>
                  {t('clubs.table.foundingDate', 'Founding Date')}{sortIcon('foundingDate')}
                </th>
                <th className="admin-table__actions-col">{t('clubs.table.actions', 'Actions')}</th>
              </tr>
            </thead>
            <tbody>
              {!loading && sortedClubs.length === 0 && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: '#6b7280' }}>
                    {isSearching
                      ? t('clubs.noSearchResults', 'No clubs found matching your search')
                      : t('clubs.empty', 'No clubs found')}
                  </td>
                </tr>
              )}

              {loading && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: '#6b7280' }}>
                    {t('common.loading', 'Loading...')}
                  </td>
                </tr>
              )}

              {!loading && sortedClubs.map((club) => (
                <tr
                  key={club.id}
                  className={`admin-table__row--clickable ${selectedIds.has(club.id) ? 'admin-table__row--selected' : ''}`}
                  onClick={() => navigate(`/admin/clubs/${club.id}`)}
                >
                  <td className="admin-table__checkbox-col" onClick={(e) => e.stopPropagation()}>
                    <input type="checkbox" checked={selectedIds.has(club.id)} onChange={() => handleToggleSelect(club.id)} />
                  </td>
                  <td className="admin-table__name">{club.name}</td>
                  <td>{club.city || '-'}</td>
                  <td>{club.country || '-'}</td>
                  <td>{formatDmy(club.foundingDate)}</td>
                  <td className="admin-table__actions-col" onClick={(e) => e.stopPropagation()}>
                    <ActionsDropdown
                      ariaLabel={t('clubs.table.actionsMenu', 'Club actions menu')}
                      actions={[
                        { label: t('common.edit', 'Edit'), onClick: () => navigate(`/admin/clubs/${club.id}/edit`) },
                      ]}
                    />
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
