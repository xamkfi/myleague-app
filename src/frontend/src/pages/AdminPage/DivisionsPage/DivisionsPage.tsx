import { useEffect, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import SearchField from '../../../components/SearchField';
import Button from '../../../components/Button/Button';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import AddIcon from '../../../assets/basicIcons/add.svg';
import { divisionService } from '../../../api/common/divisionService';
import { mapDeletionError } from '../../../utils/mapDeletionError';
import type { DivisionType } from '../../../types/common/divisionType';
import type { DivisionStatusFilter } from '../../../types/common/divisionUiTypes';
import { ACTIVE_SPORTS, SportsCategory, SPORT_LABELS } from '../../../types/common/sports';
import DivisionsTable from './components/DivisionsTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import './DivisionsPage.scss';

const DivisionsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [sportFilter, setSportFilter] = useState<SportsCategory | 'all'>('all');
  const [statusFilter, setStatusFilter] = useState<DivisionStatusFilter>('all');
  const [statusUpdatingId, setStatusUpdatingId] = useState<string | null>(null);
  const [divisionToDelete, setDivisionToDelete] = useState<DivisionType | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const loadDivisions = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await divisionService.getAll();
      setDivisions(response.data || []);
    } catch (err) {
      console.error('Failed to load divisions', err);
      setError(
        err instanceof Error
          ? err.message
          : t('admin.divisions.errors.load', 'Failed to load divisions. Please try again.'),
      );
      setDivisions([]);
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    loadDivisions();
  }, [loadDivisions]);

  const filteredDivisions = useMemo(() => {
    return divisions.filter((division) => {
      const matchesSearch =
        !searchTerm ||
        division.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        division.description.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesSport =
        sportFilter === 'all' || division.sportType === sportFilter;

      const matchesStatus =
        statusFilter === 'all' ||
        (statusFilter === 'active' && division.isActive) ||
        (statusFilter === 'inactive' && !division.isActive);

      return matchesSearch && matchesSport && matchesStatus;
    });
  }, [divisions, searchTerm, sportFilter, statusFilter]);

  const handleToggleStatus = async (division: DivisionType) => {
    try {
      setStatusUpdatingId(division.id);
      setError(null);

      if (division.isActive) {
        await divisionService.deactivate(division.id);
      } else {
        await divisionService.activate(division.id);
      }

      setDivisions((prev) =>
        prev.map((item) =>
          item.id === division.id ? { ...item, isActive: !item.isActive } : item,
        ),
      );
    } catch (err) {
      console.error('Failed to update division status', err);
      setError(
        err instanceof Error
          ? err.message
          : t('admin.divisions.errors.updateStatus', 'Failed to update status. Please try again.'),
      );
    } finally {
      setStatusUpdatingId(null);
    }
  };

  const openDeleteModal = (division: DivisionType) => {
    setDivisionToDelete(division);
    setIsDeleteModalOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setDivisionToDelete(null);
  };

  const handleConfirmDelete = async () => {
    if (!divisionToDelete) return;

    try {
      setIsDeleting(true);
      setError(null);
      await divisionService.delete(divisionToDelete.id);
      setDivisions((prev) => prev.filter((division) => division.id !== divisionToDelete.id));
      closeDeleteModal();
    } catch (err) {
      console.error('Failed to delete division', err);
      setError(
        mapDeletionError(err, t) ??
          t('admin.divisions.errors.delete', 'Failed to delete division. Please try again.'),
      );
    } finally {
      setIsDeleting(false);
    }
  };

  const handleToggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSelectAll = () => {
    setSelectedIds(new Set(filteredDivisions.map((d) => d.id)));
  };

  const handleClearSelection = () => {
    setSelectedIds(new Set());
  };

  const handleBulkActivate = async () => {
    for (const id of selectedIds) {
      const division = divisions.find((d) => d.id === id);
      if (division && !division.isActive) {
        await handleToggleStatus(division);
      }
    }
    setSelectedIds(new Set());
  };

  const handleBulkDeactivate = async () => {
    for (const id of selectedIds) {
      const division = divisions.find((d) => d.id === id);
      if (division && division.isActive) {
        await handleToggleStatus(division);
      }
    }
    setSelectedIds(new Set());
  };

  const handleBulkDelete = async () => {
    for (const id of selectedIds) {
      try {
        await divisionService.delete(id);
        setDivisions((prev) => prev.filter((d) => d.id !== id));
      } catch (err) {
        console.error('Failed to delete division', err);
        setError(
          mapDeletionError(err, t) ??
            t('admin.divisions.errors.delete', 'Failed to delete division. Please try again.'),
        );
        return;
      }
    }
    setSelectedIds(new Set());
  };

  if (loading) {
    return (
      <PageTemplate title={t('admin.divisions.title', 'Manage Divisions')}>
        <div className="divisions-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('admin.divisions.title', 'Manage Divisions')}>
      <div className="divisions-page">

        <div className="divisions-page__header">
          <div>
            <h2>{t('admin.divisions.manageTitle', 'Manage divisions')}</h2>
            <p className="divisions-page__subtitle">
              {t(
                'admin.divisions.subtitle',
                'Create, edit, activate or remove sport divisions across the league.',
              )}
            </p>
          </div>
          <Button
            className="divisions-page__create-button"
            iconLeft={AddIcon}
            rounded="pill"
            to="/admin/divisions/create"
          >
            {t('admin.divisions.actions.create', 'Create division')}
          </Button>
        </div>

        <div className="divisions-page__filters">
          <SearchField
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder={t('admin.divisions.search', 'Search by name or description...')}
            fullWidth
          />

          <div className="filter-group">
            <label htmlFor="sportFilter">{t('admin.divisions.filters.sport', 'Sport')}</label>
            <select
              id="sportFilter"
              value={sportFilter}
              onChange={(event) =>
                setSportFilter(
                  event.target.value === 'all'
                    ? 'all'
                    : (event.target.value as SportsCategory),
                )
              }
            >
              <option value="all">{t('common.all', 'All')}</option>
              {ACTIVE_SPORTS.map((sport) => (
                <option key={sport} value={sport}>
                  {t(`sports.${sport.toLowerCase()}`, SPORT_LABELS[sport])}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label htmlFor="statusFilter">{t('admin.divisions.filters.status', 'Status')}</label>
            <select
              id="statusFilter"
              value={statusFilter}
              onChange={(event) =>
                setStatusFilter(event.target.value as DivisionStatusFilter)
              }
            >
              <option value="all">{t('common.all', 'All')}</option>
              <option value="active">{t('common.active', 'Active')}</option>
              <option value="inactive">{t('common.inactive', 'Inactive')}</option>
            </select>
          </div>
        </div>

        <ErrorPopup message={error} />

        <DivisionsTable
          divisions={filteredDivisions}
          onEdit={(divisionId) => navigate(`/admin/divisions/${divisionId}/edit`)}
          onDelete={(division) => openDeleteModal(division)}
          onToggleStatus={(division) => handleToggleStatus(division)}
          statusUpdatingId={statusUpdatingId}
          selectedIds={selectedIds}
          onToggleSelect={handleToggleSelect}
          onSelectAll={handleSelectAll}
          onClearSelection={handleClearSelection}
          onBulkDelete={handleBulkDelete}
          onBulkActivate={handleBulkActivate}
          onBulkDeactivate={handleBulkDeactivate}
        />

        {filteredDivisions.length === 0 && (
          <div className="divisions-page__empty-state">
            <p>
              {searchTerm || sportFilter !== 'all' || statusFilter !== 'all'
                ? t('admin.divisions.emptyFiltered', 'No divisions match your filters.')
                : t('admin.divisions.emptyDefault', 'No divisions found yet.')}
            </p>
          </div>
        )}
      </div>

      <ConfirmDeleteModal
        isOpen={isDeleteModalOpen}
        division={divisionToDelete}
        onCancel={closeDeleteModal}
        onConfirm={handleConfirmDelete}
        isDeleting={isDeleting}
      />
    </PageTemplate>
  );
};

export default DivisionsPage;

