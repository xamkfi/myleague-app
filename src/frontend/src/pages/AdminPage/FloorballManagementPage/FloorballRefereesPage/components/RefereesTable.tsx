import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { FloorballRefereeDto } from '../../../../../api/floorball/floorballRefereeService';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import '../../../../../styles/AdminTable.scss';

interface RefereesTableProps {
  referees: FloorballRefereeDto[];
  onDelete: (refereeId: string) => void;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

const RefereesTable = ({
  referees,
  onDelete,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: RefereesTableProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  // Defensive programming: ensure referees is always an array
  const safeReferees = Array.isArray(referees) ? referees : [];

  if (safeReferees.length === 0) {
    return <div className="admin-table__empty">{t('floorball.referees.noReferees', 'No referees found.')}</div>;
  }

  const getLicenseStatus = (expiryDate?: string) => {
    if (!expiryDate) return 'unknown';
    
    const expiry = new Date(expiryDate);
    const today = new Date();
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(today.getDate() + 30);

    if (expiry < today) return 'expired';
    if (expiry < thirtyDaysFromNow) return 'expiring';
    return 'valid';
  };

  const getLicenseBadgeClass = (status: string) => {
    switch (status) {
      case 'valid':
        return 'admin-badge admin-badge--active';
      case 'expired':
        return 'admin-badge admin-badge--danger';
      case 'expiring':
        return 'admin-badge admin-badge--warning';
      default:
        return 'admin-badge admin-badge--inactive';
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleDateString();
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={safeReferees.length > 0 && safeReferees.every((r) => selectedIds.has(r.id))}
              onChange={(e) => {
                if (e.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('floorball.referees.selectAll', 'Select all referees')}
            />
          </th>
          <th>{t('floorball.referees.table.name', 'Name')}</th>
          <th>{t('floorball.referees.table.status', 'Status')}</th>
          <th>{t('floorball.referees.table.licenseExpiry', 'License Expiry')}</th>
          <th>{t('floorball.referees.table.matchesOfficiated', 'Matches')}</th>
          <th className="admin-table__actions-col">{t('floorball.referees.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {safeReferees.map((referee) => {
          const licenseStatus = getLicenseStatus(referee.licenseExpiryDate);
          
          return (
            <tr
              key={referee.id}
              className={`admin-table__row--clickable${selectedIds.has(referee.id) ? ' admin-table__row--selected' : ''}`}
              onClick={() => onToggleSelect(referee.id)}
            >
              <td className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={selectedIds.has(referee.id)}
                  onChange={() => onToggleSelect(referee.id)}
                  onClick={(e) => e.stopPropagation()}
                />
              </td>
              <td className="admin-table__name">
                {[referee.person.firstName, referee.person.lastName].filter(Boolean).join(' ') || '-'}
              </td>
              <td>
                <span
                  className={`admin-badge ${referee.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                  aria-label={referee.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                  title={referee.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                >
                  <img
                    src={referee.isActive ? CheckIcon : CloseIcon}
                    alt={referee.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                    className="status-icon"
                  />
                </span>
              </td>
              <td>
                <div>
                  <div>{formatDate(referee.licenseExpiryDate)}</div>
                  {referee.licenseExpiryDate && (
                    <span className={getLicenseBadgeClass(licenseStatus)}>
                      {licenseStatus === 'expired' && t('floorball.referees.license.expired', 'Expired')}
                      {licenseStatus === 'expiring' && t('floorball.referees.license.expiring', 'Expiring Soon')}
                      {licenseStatus === 'valid' && t('floorball.referees.license.valid', 'Valid')}
                    </span>
                  )}
                </div>
              </td>
              <td>{referee.matchesOfficiated}</td>
              <td className="admin-table__actions-col">
                <ActionsDropdown
                  actions={[
                    {
                      label: t('common.edit', 'Edit'),
                      onClick: () => navigate(`/admin/floorball/referees/${referee.id}/edit`),
                    },
                    {
                      label: t('common.delete', 'Delete'),
                      onClick: () => onDelete(referee.id),
                      variant: 'danger',
                    },
                  ]}
                  ariaLabel={t('floorball.referees.actions.menu', 'Referee actions menu')}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};

export default RefereesTable;
