import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { hockeyOfficialService } from '../../../../api/hockey/hockeyOfficialService';
import type { HockeyOfficialDto } from '../../../../types/hockey/hockeyTypes';
import OfficialsTable from './components/OfficialsTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { loadPersonNameMap } from '../../../../utils/hockeyLookups';
import '../../../../styles/AdminTable.scss';
import './HockeyOfficialsPage.scss';

function HockeyOfficialsPage() {
  const { t } = useTranslation();
  const [officials, setOfficials] = useState<HockeyOfficialDto[]>([]);
  const [names, setNames] = useState<Map<string, string>>(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [officialToDeactivate, setOfficialToDeactivate] = useState<HockeyOfficialDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const filteredOfficials = useMemo(() => {
    if (!searchTerm) {
      return officials;
    }
    const searchLower = searchTerm.toLowerCase().trim();
    return officials.filter((official) => {
      const name = names.get(official.personId) ?? '';
      return `${name} ${official.officialRole}`.toLowerCase().includes(searchLower);
    });
  }, [officials, names, searchTerm]);

  useEffect(() => {
    const fetchOfficials = async (): Promise<void> => {
      try {
        setLoading(true);
        const list = await hockeyOfficialService.getAll();
        setOfficials(list);
        setNames(await loadPersonNameMap(list.map((item) => item.personId)));
        setError(null);
      } catch {
        setOfficials([]);
        setError(t('hockey.officials.errors.loadOfficials', 'Failed to load referees. Please try again.'));
      } finally {
        setLoading(false);
      }
    };
    void fetchOfficials();
  }, [t]);

  const toggleSelect = (id: string): void => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const selectAll = (): void => {
    setSelectedIds(new Set(filteredOfficials.map((item) => item.id)));
  };

  const clearSelection = (): void => {
    setSelectedIds(new Set());
  };

  const updateActive = async (official: HockeyOfficialDto, isActive: boolean): Promise<HockeyOfficialDto> => {
    return hockeyOfficialService.update(official.id, {
      officialRole: official.officialRole,
      officialNumber: official.officialNumber,
      licenseIssueDate: official.licenseIssueDate,
      licenseExpiryDate: official.licenseExpiryDate,
      isActive,
    });
  };

  const handleDeactivate = (official: HockeyOfficialDto): void => {
    setOfficialToDeactivate(official);
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDeactivate = async (): Promise<void> => {
    if (!officialToDeactivate) {
      return;
    }
    try {
      setIsDeleting(true);
      setError(null);
      const updated = await updateActive(officialToDeactivate, false);
      setOfficials((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
      setSelectedIds((prev) => {
        const next = new Set(prev);
        next.delete(officialToDeactivate.id);
        return next;
      });
      setIsDeleteModalOpen(false);
      setOfficialToDeactivate(null);
    } catch {
      setError(t('hockey.officials.errors.deactivateFailed', 'Failed to deactivate referee. Please try again.'));
    } finally {
      setIsDeleting(false);
    }
  };

  const handleActivate = async (official: HockeyOfficialDto): Promise<void> => {
    try {
      const updated = await updateActive(official, true);
      setOfficials((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
    } catch {
      setError(t('hockey.officials.errors.activateFailed', 'Failed to activate referee. Please try again.'));
    }
  };

  const handleBulkDeactivate = async (): Promise<void> => {
    if (selectedIds.size === 0) {
      return;
    }
    try {
      setError(null);
      const selected = officials.filter((item) => selectedIds.has(item.id) && item.isActive);
      for (const official of selected) {
        const updated = await updateActive(official, false);
        setOfficials((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
      }
      setSelectedIds(new Set());
    } catch {
      setError(t('hockey.officials.errors.bulkDeactivateFailed', 'Failed to deactivate selected referees. Please try again.'));
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('hockey.officials.title', 'Manage Floorball Referees')}>
        <div className="floorball-referees-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.officials.title', 'MANAGE REFEREES')}>
      <div className="floorball-referees-container">
        <h2 className="floorball-referees-title">{t('hockey.officials.title', 'MANAGE REFEREES')}</h2>
        <div className="floorball-referees-header">
          <div className="referees-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('hockey.officials.search', 'Search referees...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="create-referee-button"
              iconLeft={AddIcon}
              to="/admin/hockey/officials/create"
            >
              {t('hockey.officials.create', 'Create new referee')}
            </Button>
          </div>
        </div>
        <ErrorPopup message={error} />
        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={filteredOfficials.length}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('hockey.officials.bulkDeactivate', 'Deactivate ({{count}})', { count: selectedIds.size }),
              onClick: () => void handleBulkDeactivate(),
              variant: 'danger',
            },
          ]}
        />
        <div className="admin-table__wrapper">
          <OfficialsTable
            officials={filteredOfficials}
            names={names}
            onDeactivate={handleDeactivate}
            onActivate={(official) => void handleActivate(official)}
            selectedIds={selectedIds}
            onToggleSelect={toggleSelect}
            onSelectAll={selectAll}
            onClearSelection={clearSelection}
          />
        </div>
        {filteredOfficials.length === 0 && !loading && (
          <div className="no-data">
            {searchTerm
              ? t('hockey.officials.noSearchResults', 'No referees found matching "{{searchTerm}}"', { searchTerm })
              : t('hockey.officials.noOfficials', 'No referees found.')}
          </div>
        )}
        <ConfirmDeleteModal
          isOpen={isDeleteModalOpen}
          official={officialToDeactivate}
          officialName={officialToDeactivate ? names.get(officialToDeactivate.personId) ?? officialToDeactivate.personId.slice(0, 8) : ''}
          onConfirm={() => void handleConfirmDeactivate()}
          onCancel={() => {
            setIsDeleteModalOpen(false);
            setOfficialToDeactivate(null);
          }}
          isDeleting={isDeleting}
        />
      </div>
    </PageTemplate>
  );
}

export default HockeyOfficialsPage;
