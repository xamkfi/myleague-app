import { useTranslation } from 'react-i18next';
import type { SystemUser } from '../../../../types/admin/userTypes';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';

interface UsersTableProps {
  users: SystemUser[];
  onEdit: (user: SystemUser) => void;
  onDelete: (user: SystemUser) => void;
  onResendInvitation: (user: SystemUser) => void;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
}

const UsersTable = ({
  users,
  onEdit,
  onDelete,
  onResendInvitation,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
  onBulkDelete,
}: UsersTableProps) => {
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
    return role === 'SystemAdmin' ? 'admin-badge--system' : 'admin-badge--club';
  };

  const getRoleLabel = (role: string) => {
    return role === 'SystemAdmin'
      ? t('admin.users.roles.systemAdmin', 'System Admin')
      : t('admin.users.roles.clubAdmin', 'Club Admin');
  };

  const allSelected = users.length > 0 && selectedIds.size === users.length;

  return (
    <>
      <BulkActionsBar
        selectedCount={selectedIds.size}
        totalCount={users.length}
        onSelectAll={onSelectAll}
        onClearSelection={onClearSelection}
        actions={[
          {
            label: t('common.delete', 'Delete'),
            onClick: onBulkDelete,
            variant: 'danger',
          },
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
                  onChange={allSelected ? onClearSelection : onSelectAll}
                />
              </th>
              <th>{t('admin.users.table.person', 'Person')}</th>
              <th>{t('admin.users.table.email', 'Email')}</th>
              <th>{t('admin.users.table.role', 'Role')}</th>
              <th>{t('admin.users.table.status', 'Status')}</th>
              <th>{t('admin.users.table.emailVerified', 'Email Verified')}</th>
              <th>{t('admin.users.table.lastLogin', 'Last Login')}</th>
              <th className="admin-table__actions-col">
                {t('common.actions', 'Actions')}
              </th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => {
              const isSelected = selectedIds.has(user.id);
              return (
                <tr
                  key={user.id}
                  className={isSelected ? 'admin-table__row--selected' : ''}
                >
                  <td className="admin-table__checkbox-col">
                    <input
                      type="checkbox"
                      checked={isSelected}
                      onChange={() => onToggleSelect(user.id)}
                    />
                  </td>
                  <td>
                    <div className="admin-table__name">
                      {user.person?.fullName ?? '—'}
                    </div>
                  </td>
                  <td>{user.email}</td>
                  <td>
                    <span className={`admin-badge ${getRoleBadgeClass(user.role)}`}>
                      {getRoleLabel(user.role)}
                    </span>
                  </td>
                  <td>
                    <span
                      className={`admin-badge ${user.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                    >
                      {user.isActive
                        ? t('common.active', 'Active')
                        : t('common.inactive', 'Inactive')}
                    </span>
                  </td>
                  <td>
                    <span
                      className={`admin-badge ${user.isEmailVerified ? 'admin-badge--active' : 'admin-badge--pending'}`}
                    >
                      {user.isEmailVerified
                        ? t('admin.users.table.verified', 'Verified')
                        : t('admin.users.table.pending', 'Pending')}
                    </span>
                  </td>
                  <td className="admin-table__muted">{formatDate(user.lastLoginAt)}</td>
                  <td className="admin-table__actions-col">
                    <ActionsDropdown
                      ariaLabel={t('admin.users.actions.menu', 'User actions menu')}
                      actions={[
                        { label: t('common.edit', 'Edit'), onClick: () => onEdit(user) },
                        ...(!user.isEmailVerified
                          ? [{
                              label: t('admin.users.actions.resendInvitation', 'Resend Invitation'),
                              onClick: () => onResendInvitation(user),
                            }]
                          : []),
                        { label: t('common.delete', 'Delete'), onClick: () => onDelete(user), variant: 'danger' as const },
                      ]}
                    />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </>
  );
};

export default UsersTable;
