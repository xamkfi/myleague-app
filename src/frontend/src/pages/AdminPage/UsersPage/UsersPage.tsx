import { useEffect, useMemo, useState, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import SearchField from '../../../components/SearchField';
import Button from '../../../components/Button/Button';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import AddIcon from '../../../assets/basicIcons/add.svg';
import { userService } from '../../../api/admin/userService';
import { mapDeletionError } from '../../../utils/mapDeletionError';
import type { SystemUser, UserRole } from '../../../types/admin/userTypes';
import UsersTable from './components/UsersTable';
import UserFormModal from './components/UserFormModal';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import './UsersPage.scss';

type StatusFilter = 'all' | 'active' | 'inactive';

const UsersPage = () => {
  const { t } = useTranslation();

  const [users, setUsers] = useState<SystemUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');

  // Selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Form modal state
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<SystemUser | null>(null);

  // Delete modal state
  const [userToDelete, setUserToDelete] = useState<SystemUser | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Resend invitation state
  const [resendingUserId, setResendingUserId] = useState<string | null>(null);
  const [resendSuccess, setResendSuccess] = useState<string | null>(null);
  const resendTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const loadUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await userService.getAll();
      setUsers(data || []);
    } catch (err) {
      console.error('Failed to load users', err);
      setError(
        err instanceof Error
          ? err.message
          : t('admin.users.errors.load', 'Failed to load users. Please try again.'),
      );
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  const filteredUsers = useMemo(() => {
    const term = searchTerm.toLowerCase();
    return users.filter((user) => {
      const matchesSearch =
        !term ||
        user.email.toLowerCase().includes(term) ||
        (user.person?.fullName ?? '').toLowerCase().includes(term) ||
        (user.person?.firstName ?? '').toLowerCase().includes(term) ||
        (user.person?.lastName ?? '').toLowerCase().includes(term);

      const matchesStatus =
        statusFilter === 'all' ||
        (statusFilter === 'active' && user.isActive) ||
        (statusFilter === 'inactive' && !user.isActive);

      return matchesSearch && matchesStatus;
    });
  }, [users, searchTerm, statusFilter]);

  // Collect existing person IDs so the form modal can exclude them
  const existingPersonIds = useMemo(() => users.map((u) => u.personId), [users]);

  // --- Form modal handlers ---

  const openCreateModal = () => {
    setEditingUser(null);
    setIsFormModalOpen(true);
  };

  const openEditModal = (user: SystemUser) => {
    setEditingUser(user);
    setIsFormModalOpen(true);
  };

  const closeFormModal = () => {
    setIsFormModalOpen(false);
    setEditingUser(null);
  };

  const handleSaveUser = async (
    email: string,
    personId: string,
    role: UserRole,
    isActive: boolean,
    clubAssignments?: string[],
  ) => {
    try {
      setError(null);

      if (editingUser) {
        const updated = await userService.update(editingUser.id, { email, role, isActive });
        setUsers((prev) =>
          prev.map((u) => (u.id === updated.id ? updated : u)),
        );
      } else {
        const created = await userService.create({ email, personId, role, clubAssignments });
        setUsers((prev) => [...prev, created]);
      }

      closeFormModal();
    } catch (err) {
      console.error('Failed to save user', err);
      setError(
        err instanceof Error
          ? err.message
          : editingUser
            ? t('admin.users.errors.update', 'Failed to update user. Please try again.')
            : t('admin.users.errors.create', 'Failed to create user. Please try again.'),
      );
      throw err;
    }
  };

  // --- Delete modal handlers ---

  const openDeleteModal = (user: SystemUser) => {
    setUserToDelete(user);
    setIsDeleteModalOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setUserToDelete(null);
  };

  const handleConfirmDelete = async () => {
    if (!userToDelete) return;

    try {
      setIsDeleting(true);
      setError(null);
      await userService.delete(userToDelete.id);
      setUsers((prev) => prev.filter((u) => u.id !== userToDelete.id));
      closeDeleteModal();
    } catch (err) {
      console.error('Failed to delete user', err);
      setError(
        mapDeletionError(err, t) ??
          t('admin.users.errors.delete', 'Failed to delete user. Please try again.'),
      );
    } finally {
      setIsDeleting(false);
    }
  };

  // --- Revoke club admin handler ---

  const handleRevokeClubAdmin = useCallback(async (user: SystemUser) => {
    const confirmed = window.confirm(
      t(
        'admin.users.confirmRevokeClubAdmin',
        'Revoke club admin access for {{email}}? Their account will be deactivated and they will no longer be able to sign in.',
        { email: user.email },
      ),
    );
    if (!confirmed) return;

    try {
      setError(null);
      const updated = await userService.update(user.id, {
        email: user.email,
        role: user.role,
        isActive: false,
      });
      setUsers((prev) => prev.map((u) => (u.id === updated.id ? updated : u)));
    } catch (err) {
      console.error('Failed to revoke club admin access', err);
      setError(
        err instanceof Error
          ? err.message
          : t('admin.users.errors.revoke', 'Failed to revoke club admin access. Please try again.'),
      );
    }
  }, [t]);

  // --- Resend invitation handler ---

  const handleResendInvitation = useCallback(async (user: SystemUser) => {
    try {
      setError(null);
      setResendSuccess(null);
      if (resendTimerRef.current) clearTimeout(resendTimerRef.current);
      setResendingUserId(user.id);
      await userService.resendInvitation(user.id);
      setResendSuccess(
        t('admin.users.invitationResent', 'Invitation email resent to {{email}}.', { email: user.email })
      );
      resendTimerRef.current = setTimeout(() => setResendSuccess(null), 5000);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('admin.users.errors.resend', 'Failed to resend invitation. Please try again.'),
      );
    } finally {
      setResendingUserId(null);
    }
  }, [t]);

  useEffect(() => {
    return () => {
      if (resendTimerRef.current) clearTimeout(resendTimerRef.current);
    };
  }, []);

  // --- Selection handlers ---

  const handleToggleSelect = useCallback((id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const handleSelectAll = useCallback(() => {
    setSelectedIds(new Set(filteredUsers.map((u) => u.id)));
  }, [filteredUsers]);

  const handleClearSelection = useCallback(() => {
    setSelectedIds(new Set());
  }, []);

  const handleBulkDelete = useCallback(async () => {
    if (selectedIds.size === 0) return;

    try {
      setError(null);
      await Promise.all(
        Array.from(selectedIds).map((id) => userService.delete(id)),
      );
      setUsers((prev) => prev.filter((u) => !selectedIds.has(u.id)));
      setSelectedIds(new Set());
    } catch (err) {
      console.error('Failed to bulk delete users', err);
      setError(
        mapDeletionError(err, t) ??
          t('admin.users.errors.delete', 'Failed to delete user. Please try again.'),
      );
    }
  }, [selectedIds, t]);

  // --- Render ---

  if (loading) {
    return (
      <PageTemplate title={t('admin.users.title', 'System Users')}>
        <div className="users-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('admin.users.title', 'System Users')}>
      <div className="users-page">
        <div className="users-page__header">
          <div>
            <h2 className="page-title-compact font-title">
              {t('admin.users.title', 'System Users')}
            </h2>
            <p className="users-page__subtitle">
              {t(
                'admin.users.subtitle',
                'Create, edit or remove system user accounts.',
              )}
            </p>
          </div>
          <Button
            className="users-page__create-button"
            iconLeft={AddIcon}
            rounded="pill"
            onClick={openCreateModal}
          >
            {t('admin.users.addUser', 'Add User')}
          </Button>
        </div>

        <div className="users-page__filters">
          <SearchField
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder={t(
              'admin.users.searchPlaceholder',
              'Search by name or email...',
            )}
            fullWidth
          />

          <div className="filter-group">
            <label htmlFor="statusFilter">
              {t('admin.users.table.status', 'Status')}
            </label>
            <select
              id="statusFilter"
              value={statusFilter}
              onChange={(event) =>
                setStatusFilter(event.target.value as StatusFilter)
              }
            >
              <option value="all">{t('common.all', 'All')}</option>
              <option value="active">{t('common.active', 'Active')}</option>
              <option value="inactive">
                {t('common.inactive', 'Inactive')}
              </option>
            </select>
          </div>
        </div>

        <ErrorPopup message={error} />

        {resendSuccess && (
          <div className="users-page__success-notice">
            {resendSuccess}
          </div>
        )}

        <UsersTable
          users={filteredUsers}
          onEdit={openEditModal}
          onDelete={openDeleteModal}
          onResendInvitation={handleResendInvitation}
          onRevokeClubAdmin={handleRevokeClubAdmin}
          resendingUserId={resendingUserId}
          selectedIds={selectedIds}
          onToggleSelect={handleToggleSelect}
          onSelectAll={handleSelectAll}
          onClearSelection={handleClearSelection}
          onBulkDelete={handleBulkDelete}
        />

        {filteredUsers.length === 0 && !loading && (
          <div className="users-page__empty-state">
            <p>
              {searchTerm || statusFilter !== 'all'
                ? t(
                    'admin.users.noSearchResults',
                    'No users match your filters.',
                  )
                : t('admin.users.noData', 'No system users found yet.')}
            </p>
          </div>
        )}
      </div>

      <UserFormModal
        isOpen={isFormModalOpen}
        user={editingUser}
        existingPersonIds={existingPersonIds}
        onSave={handleSaveUser}
        onCancel={closeFormModal}
        onResendInvitation={handleResendInvitation}
        isResendingInvitation={resendingUserId !== null}
      />

      <ConfirmDeleteModal
        isOpen={isDeleteModalOpen}
        user={userToDelete}
        onCancel={closeDeleteModal}
        onConfirm={handleConfirmDelete}
        isDeleting={isDeleting}
      />
    </PageTemplate>
  );
};

export default UsersPage;
