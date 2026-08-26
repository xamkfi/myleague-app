import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { HockeyOfficialDto } from '../../../../../types/hockey/hockeyTypes';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import '../../../../../styles/AdminTable.scss';

interface OfficialsTableProps {
  officials: HockeyOfficialDto[];
  names: Map<string, string>;
  onDeactivate: (official: HockeyOfficialDto) => void;
  onActivate: (official: HockeyOfficialDto) => void;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

function OfficialsTable({
  officials,
  names,
  onDeactivate,
  onActivate,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: OfficialsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const safeOfficials = Array.isArray(officials) ? officials : [];

  if (safeOfficials.length === 0) {
    return <div className="admin-table__empty">{t('hockey.officials.noOfficials', 'No referees found.')}</div>;
  }

  const getLicenseStatus = (expiryDate: string | null): 'expired' | 'expiring' | 'valid' | 'unknown' => {
    if (!expiryDate) {
      return 'unknown';
    }
    const expiry = new Date(expiryDate);
    const today = new Date();
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(today.getDate() + 30);
    if (expiry < today) {
      return 'expired';
    }
    if (expiry < thirtyDaysFromNow) {
      return 'expiring';
    }
    return 'valid';
  };

  const getLicenseBadgeClass = (status: string): string => {
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

  const formatDate = (dateStr: string | null): string => {
    if (!dateStr) {
      return 'N/A';
    }
    return new Date(dateStr).toLocaleDateString();
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={safeOfficials.length > 0 && safeOfficials.every((item) => selectedIds.has(item.id))}
              onChange={(event) => {
                if (event.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('hockey.officials.selectAll', 'Select all referees')}
            />
          </th>
          <th>{t('hockey.officials.table.name', 'Name')}</th>
          <th>{t('hockey.officials.role', 'Role')}</th>
          <th>{t('hockey.officials.table.status', 'Status')}</th>
          <th>{t('hockey.officials.table.licenseExpiry', 'License Expiry')}</th>
          <th>{t('hockey.officials.table.matchesOfficiated', 'Matches')}</th>
          <th className="admin-table__actions-col">{t('hockey.officials.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {safeOfficials.map((official) => {
          const licenseStatus = getLicenseStatus(official.licenseExpiryDate);
          return (
            <tr
              key={official.id}
              className={`admin-table__row--clickable${selectedIds.has(official.id) ? ' admin-table__row--selected' : ''}`}
              onClick={() => onToggleSelect(official.id)}
            >
              <td className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={selectedIds.has(official.id)}
                  onChange={() => onToggleSelect(official.id)}
                  onClick={(event) => event.stopPropagation()}
                />
              </td>
              <td className="admin-table__name">
                {names.get(official.personId) ?? official.personId.slice(0, 8)}
              </td>
              <td>
                <span className="admin-tag admin-tag--blue">
                  {t(`hockey.officials.roles.${official.officialRole}`, official.officialRole)}
                </span>
              </td>
              <td>
                <span
                  className={`admin-badge ${official.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                  title={official.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                >
                  <img
                    src={official.isActive ? CheckIcon : CloseIcon}
                    alt={official.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                    className="status-icon"
                  />
                </span>
              </td>
              <td>
                <div>
                  <div>{formatDate(official.licenseExpiryDate)}</div>
                  {official.licenseExpiryDate && (
                    <span className={getLicenseBadgeClass(licenseStatus)}>
                      {licenseStatus === 'expired' && t('hockey.officials.license.expired', 'Expired')}
                      {licenseStatus === 'expiring' && t('hockey.officials.license.expiring', 'Expiring Soon')}
                      {licenseStatus === 'valid' && t('hockey.officials.license.valid', 'Valid')}
                    </span>
                  )}
                </div>
              </td>
              <td>{official.matchesOfficiated}</td>
              <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
                <ActionsDropdown
                  actions={[
                    {
                      label: t('common.edit', 'Edit'),
                      onClick: () => navigate(`/admin/hockey/officials/${official.id}/edit`),
                    },
                    official.isActive
                      ? {
                          label: t('common.deactivate', 'Deactivate'),
                          onClick: () => onDeactivate(official),
                          variant: 'danger' as const,
                        }
                      : {
                          label: t('hockey.officials.activate', 'Activate'),
                          onClick: () => onActivate(official),
                          variant: 'status' as const,
                        },
                  ]}
                  ariaLabel={t('hockey.officials.actions.menu', 'Referee actions menu')}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}

export default OfficialsTable;
