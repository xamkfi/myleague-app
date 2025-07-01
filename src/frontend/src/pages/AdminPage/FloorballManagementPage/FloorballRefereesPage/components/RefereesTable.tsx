import { useTranslation } from 'react-i18next';
import type { FloorballRefereeDto } from '../../../../../api/floorball/floorballRefereeService';

interface RefereesTableProps {
  referees: FloorballRefereeDto[];
  onDelete: (refereeId: string) => void;
}

const RefereesTable = ({ referees, onDelete }: RefereesTableProps) => {
  const { t } = useTranslation();

  // Defensive programming: ensure referees is always an array
  const safeReferees = Array.isArray(referees) ? referees : [];

  if (safeReferees.length === 0) {
    return <div className="no-data-state">{t('floorball.referees.noReferees', 'No referees found.')}</div>;
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

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleDateString();
  };

  return (
    <table className="referees-table">
      <thead>
        <tr>
          <th>{t('floorball.referees.table.name', 'Name')}</th>
          <th>{t('floorball.referees.table.status', 'Status')}</th>
          <th>{t('floorball.referees.table.licenseExpiry', 'License Expiry')}</th>
          <th>{t('floorball.referees.table.matchesOfficiated', 'Matches')}</th>
          <th>{t('floorball.referees.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {safeReferees.map((referee) => {
          const licenseStatus = getLicenseStatus(referee.licenseExpiryDate);
          
          return (
            <tr key={referee.id}>
              <td>{referee.person.fullName}</td>
              <td>
                <span className={`status-badge ${referee.isActive ? 'active' : 'inactive'}`}>
                  {referee.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                </span>
              </td>
              <td>
                <div>
                  <div>{formatDate(referee.licenseExpiryDate)}</div>
                  {referee.licenseExpiryDate && (
                    <span className={`license-badge ${licenseStatus}`}>
                      {licenseStatus === 'expired' && t('floorball.referees.license.expired', 'Expired')}
                      {licenseStatus === 'expiring' && t('floorball.referees.license.expiring', 'Expiring Soon')}
                      {licenseStatus === 'valid' && t('floorball.referees.license.valid', 'Valid')}
                    </span>
                  )}
                </div>
              </td>
              <td>{referee.matchesOfficiated}</td>
              <td>
                <div className="action-buttons">
                  <button onClick={() => onDelete(referee.id)} className="delete-btn">
                    {t('common.delete', 'Delete')}
                  </button>
                </div>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};

export default RefereesTable; 