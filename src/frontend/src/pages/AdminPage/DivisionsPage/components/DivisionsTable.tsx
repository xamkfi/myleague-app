import { useTranslation } from 'react-i18next';
import type { DivisionType } from '../../../../types/common/divisionType';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';
import './DivisionsTable.scss';

interface DivisionsTableProps {
  divisions: DivisionType[];
  onEdit: (divisionId: string) => void;
  onDelete: (division: DivisionType) => void;
  onToggleStatus: (division: DivisionType) => void;
  statusUpdatingId?: string | null;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  onBulkActivate: () => void;
  onBulkDeactivate: () => void;
}

const DivisionsTable = ({
  divisions,
  onEdit,
  onDelete,
  onToggleStatus,
  statusUpdatingId,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  onBulkActivate,
  onBulkDeactivate,
}: DivisionsTableProps) => {
  const { t } = useTranslation();

  if (divisions.length === 0) {
    return null;
  }

  const formatDate = (value: string) => {
    const parsedDate = new Date(value);
    if (Number.isNaN(parsedDate.getTime())) {
      return '-';
    }
    return parsedDate.toLocaleDateString();
  };

  const allSelected = divisions.length > 0 && divisions.every((d) => selectedIds.has(d.id));

  return (
    <>
      <BulkActionsBar
        selectedCount={selectedIds.size}
        totalCount={divisions.length}
        onSelectAll={onSelectAll}
        onClearSelection={onClearSelection}
        actions={[
          { label: t('admin.divisions.actions.activate', 'Activate'), onClick: onBulkActivate, variant: 'status' },
          { label: t('admin.divisions.actions.deactivate', 'Deactivate'), onClick: onBulkDeactivate, variant: 'status' },
          { label: t('common.delete', 'Delete'), onClick: onBulkDelete, variant: 'danger' },
        ]}
      />
      <div className="admin-table__wrapper">
        <table className="admin-table">
          <thead>
            <tr>
              <th className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={allSelected}
                  onChange={() => (allSelected ? onClearSelection() : onSelectAll())}
                />
              </th>
              <th>{t('common.name', 'Name')}</th>
              <th>{t('admin.divisions.table.sport', 'Sport')}</th>
              <th>{t('admin.divisions.table.level', 'Level')}</th>
              <th>{t('admin.divisions.table.status', 'Status')}</th>
              <th>{t('admin.divisions.table.created', 'Created')}</th>
              <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
            </tr>
          </thead>
          <tbody>
            {divisions.map((division) => (
              <tr
                key={division.id}
                className={selectedIds.has(division.id) ? 'admin-table__row--selected' : ''}
              >
                <td className="admin-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={selectedIds.has(division.id)}
                    onChange={() => onToggleSelect(division.id)}
                  />
                </td>
                <td>
                  <div className="admin-table__name">{division.name}</div>
                  <div className="admin-table__subtitle">{division.description}</div>
                </td>
                <td className="admin-table__bold">{division.sportType}</td>
                <td className="admin-table__bold">{division.level}</td>
                <td>
                  <span
                    className={`admin-badge ${division.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                  >
                    {division.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                  </span>
                </td>
                <td>{formatDate(division.createdDate)}</td>
                <td className="admin-table__actions-col">
                  <ActionsDropdown
                    ariaLabel={t('admin.divisions.actions.menu', 'Division actions menu')}
                    actions={[
                      { label: t('common.edit', 'Edit'), onClick: () => onEdit(division.id) },
                      {
                        label: division.isActive
                          ? t('admin.divisions.actions.deactivate', 'Deactivate')
                          : t('admin.divisions.actions.activate', 'Activate'),
                        onClick: () => onToggleStatus(division),
                        variant: 'status',
                        disabled: statusUpdatingId === division.id,
                      },
                      { label: t('common.delete', 'Delete'), onClick: () => onDelete(division), variant: 'danger' },
                    ]}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
};

export default DivisionsTable;
