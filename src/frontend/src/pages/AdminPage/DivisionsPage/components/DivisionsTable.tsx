import { useTranslation } from 'react-i18next';
import type { DivisionType } from '../../../../types/common/divisionType';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import './DivisionsTable.scss';

interface DivisionsTableProps {
  divisions: DivisionType[];
  onEdit: (divisionId: string) => void;
  onDelete: (division: DivisionType) => void;
  onToggleStatus: (division: DivisionType) => void;
  statusUpdatingId?: string | null;
}

const DivisionsTable = ({
  divisions,
  onEdit,
  onDelete,
  onToggleStatus,
  statusUpdatingId,
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

  return (
    <div className="divisions-table__wrapper">
      <table className="divisions-table">
        <thead>
          <tr>
            <th>{t('common.name', 'Name')}</th>
            <th>{t('admin.divisions.table.sport', 'Sport')}</th>
            <th>{t('admin.divisions.table.level', 'Level')}</th>
            <th>{t('admin.divisions.table.status', 'Status')}</th>
            <th>{t('admin.divisions.table.created', 'Created')}</th>
            <th className="division-actions-column-header">{t('common.actions', 'Actions')}</th>
          </tr>
        </thead>
        <tbody>
          {divisions.map((division) => (
            <tr key={division.id}>
              <td>
                <div className="division-name">{division.name}</div>
                <div className="division-description">{division.description}</div>
              </td>
              <td className="division-sport">{division.sportType}</td>
              <td className="division-level">{division.level}</td>
              <td>
                <span
                  className={`division-status ${division.isActive ? 'division-status--active' : 'division-status--inactive'}`}
                >
                  {division.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                </span>
              </td>
              <td>{formatDate(division.createdDate)}</td>
              <td className="division-actions-column">
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
  );
};

export default DivisionsTable;

