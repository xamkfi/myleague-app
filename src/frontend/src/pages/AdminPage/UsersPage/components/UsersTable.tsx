import { useTranslation } from 'react-i18next';
import type { SystemUser } from '../../../../types/admin/userTypes';

interface UsersTableProps {
  users: SystemUser[];
  onEdit: (user: SystemUser) => void;
  onDelete: (user: SystemUser) => void;
}

const UsersTable = ({ users, onEdit, onDelete }: UsersTableProps) => {
  const { t } = useTranslation();

  if (users.length === 0) {
    return null;
  }

  const formatDate = (value: string | null) => {
    if (!value) {
      return t('admin.users.table.never', 'Never');
    }
    const parsedDate = new Date(value);
    if (Number.isNaN(parsedDate.getTime())) {
      return '—';
    }
    return parsedDate.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getRoleBadgeClass = (role: string) => {
    return role === 'SystemAdmin' ? 'user-role--system' : 'user-role--club';
  };

  const getRoleLabel = (role: string) => {
    return role === 'SystemAdmin'
      ? t('admin.users.roles.systemAdmin', 'System Admin')
      : t('admin.users.roles.clubAdmin', 'Club Admin');
  };

  return (
    <div className="users-table__wrapper">
      <table className="users-table">
        <thead>
          <tr>
            <th>{t('admin.users.table.person', 'Person')}</th>
            <th>{t('admin.users.table.email', 'Email')}</th>
            <th>{t('admin.users.table.role', 'Role')}</th>
            <th>{t('admin.users.table.status', 'Status')}</th>
            <th>{t('admin.users.table.lastLogin', 'Last Login')}</th>
            <th className="users-actions-column-header">
              {t('common.actions', 'Actions')}
            </th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>
                <div className="user-person-name">
                  {user.person?.fullName ?? '—'}
                </div>
              </td>
              <td className="user-email">{user.email}</td>
              <td>
                <span className={`user-role ${getRoleBadgeClass(user.role)}`}>
                  {getRoleLabel(user.role)}
                </span>
              </td>
              <td>
                <span
                  className={`user-status ${user.isActive ? 'user-status--active' : 'user-status--inactive'}`}
                >
                  {user.isActive
                    ? t('common.active', 'Active')
                    : t('common.inactive', 'Inactive')}
                </span>
              </td>
              <td className="user-last-login">{formatDate(user.lastLoginAt)}</td>
              <td className="users-actions-column">
                <div className="users-action-buttons">
                  <button
                    type="button"
                    className="users-action-btn users-action-btn--edit"
                    onClick={() => onEdit(user)}
                    title={t('common.edit', 'Edit')}
                  >
                    {t('common.edit', 'Edit')}
                  </button>
                  <button
                    type="button"
                    className="users-action-btn users-action-btn--delete"
                    onClick={() => onDelete(user)}
                    title={t('common.delete', 'Delete')}
                  >
                    {t('common.delete', 'Delete')}
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UsersTable;
