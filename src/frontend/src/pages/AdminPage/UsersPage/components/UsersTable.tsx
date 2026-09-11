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
  onRevokeClubAdmin: (user: SystemUser) => void;
  resendingUserId: string | null;
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
  onRevokeClubAdmin,
  resendingUserId,
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
      hour12: false, // <-- forces 24h
    });
  };

  const getRoleBadgeClass = (role: string) => {
    if (role === 'SystemAdmin') return 'admin-badge--system';
    return 'admin-badge--club';
  };

  const getRoleLabel = (role: string) => {
    if (role === 'SystemAdmin') return t('admin.users.roles.systemAdmin', 'System Admin');
    return t('admin.users.roles.clubAdmin', 'Club Admin');
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
              <th>{t('admin.users.table.status', 'Status')}</th>
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
                    <div className="admin-table__subtitle">
                      <span className={`admin-badge admin-badge--sm ${getRoleBadgeClass(user.role)}`}>
                        {getRoleLabel(user.role)}
                      </span>
                    </div>
                  </td>
                  <td>{user.email}</td>
                  <td>
                    <div className="users-table__status-cell">
                      <span
                        className={`admin-badge admin-badge--sm ${user.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                      >
                        {user.isActive
                          ? t('common.active', 'Active')
                          : t('common.inactive', 'Inactive')}
                      </span>
                      {!user.isEmailVerified && (
                        <span className="admin-badge admin-badge--sm admin-badge--pending">
                          {t('admin.users.table.emailPending', 'Email unverified')}
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="admin-table__muted">{formatDate(user.lastLoginAt)}</td>
                  <td className="admin-table__actions-col">
                    <ActionsDropdown
                      ariaLabel={t('admin.users.actions.menu', 'User actions menu')}
                      actions={[
                        { label: t('common.edit', 'Edit'), onClick: () => onEdit(user) },
                        ...(!user.isEmailVerified
                          ? [{
                              label: resendingUserId === user.id
                                ? t('admin.users.actions.sendingInvitation', 'Sending...')
                                : t('admin.users.actions.resendInvitation', 'Resend Invitation'),
                              onClick: () => onResendInvitation(user),
                              disabled: resendingUserId === user.id,
                            }]
                          : []),
                        ...(user.role === 'ClubAdmin' && user.isActive
                          ? [{
                              label: t('admin.users.actions.revokeClubAdmin', 'Revoke club admin access'),
                              onClick: () => onRevokeClubAdmin(user),
                              variant: 'danger' as const,
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
