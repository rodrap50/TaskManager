import { useRef, useState } from 'react';
import { Camera, Plus } from 'lucide-react';
import {
    setUserActive,
    setUserAdmin,
    uploadUserAvatar,
    resolveAssetUrl,
    getErrorMessage,
    type AppUserDto,
} from '@taskmanager/shared';
import { AssigneeAvatar } from '@taskmanager/ui';
import { useUsers } from '../../../hooks/useUsers';
import { CreateUserModal } from './CreateUserModal';
import { useUserManagementScreenStyles } from './UserManagementScreen.styles';

interface RowProps {
    user: AppUserDto;
    onChanged: () => void | Promise<void>;
}

function UserRow({ user, onChanged }: RowProps) {
    const styles = useUserManagementScreenStyles();
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const run = async (action: () => Promise<unknown>) => {
        setBusy(true);
        setError(null);
        try {
            await action();
            await onChanged();
        } catch (e) {
            setError(getErrorMessage(e, 'Action failed'));
        } finally {
            setBusy(false);
        }
    };

    const handleAvatarPicked = (file: File | undefined) => {
        if (!file) return;
        void run(() => uploadUserAvatar(user.id, file));
    };

    return (
        <>
            <tr className={`${styles.row} ${user.isActive ? '' : styles.rowInactive}`}>
                <td className={styles.td}>
                    <div className={styles.identityCell}>
                        <div className={styles.avatarWrap}>
                            <AssigneeAvatar
                                displayName={user.displayName}
                                avatarUrl={resolveAssetUrl(user.avatarUrl)}
                                size="md"
                            />
                            <button
                                type="button"
                                onClick={() => fileInputRef.current?.click()}
                                disabled={busy}
                                className={styles.avatarUploadButton}
                                title="Upload avatar"
                                aria-label={`Upload avatar for ${user.displayName}`}
                            >
                                <Camera className={styles.avatarUploadIcon} />
                            </button>
                            <input
                                ref={fileInputRef}
                                type="file"
                                accept="image/png,image/jpeg,image/webp,image/gif"
                                className={styles.hiddenFileInput}
                                onChange={e => {
                                    handleAvatarPicked(e.target.files?.[0]);
                                    e.target.value = '';
                                }}
                            />
                        </div>
                        <span className={styles.username}>{user.username}</span>
                    </div>
                </td>
                <td className={styles.td}>{user.displayName}</td>
                <td className={styles.tdMuted}>{user.email}</td>
                <td className={styles.td}>
                    <span className={`${styles.statusBadge} ${user.isActive ? styles.statusActive : styles.statusInactive}`}>
                        <span className={styles.statusDot} />
                        {user.isActive ? 'Active' : 'Inactive'}
                    </span>
                </td>
                <td className={styles.td}>
                    {user.isAdmin && <span className={styles.adminBadge}>Admin</span>}
                </td>
                <td className={styles.td}>
                    <div className={styles.actionsCell}>
                        <button
                            type="button"
                            disabled={busy}
                            onClick={() => void run(() => setUserActive(user.id, !user.isActive))}
                            className={styles.actionButton}
                        >
                            {user.isActive ? 'Deactivate' : 'Activate'}
                        </button>
                        <button
                            type="button"
                            disabled={busy}
                            onClick={() => void run(() => setUserAdmin(user.id, !user.isAdmin))}
                            className={styles.actionButton}
                        >
                            {user.isAdmin ? 'Revoke admin' : 'Promote to admin'}
                        </button>
                    </div>
                </td>
            </tr>
            {error && (
                <tr>
                    <td colSpan={6} className={styles.rowError}>{error}</td>
                </tr>
            )}
        </>
    );
}

export function UserManagementScreen() {
    const { data: users, isLoading, error, refetch } = useUsers();
    const styles = useUserManagementScreenStyles();
    const [showCreateModal, setShowCreateModal] = useState(false);

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>User Management</h2>
                <button type="button" onClick={() => setShowCreateModal(true)} className={styles.newButton}>
                    <Plus className={styles.newButtonIcon} />
                    New User
                </button>
            </div>

            {isLoading && <p className={styles.loadingText}>Loading…</p>}
            {error && <p className={styles.errorText}>Failed to load users — check the API is running</p>}

            {!isLoading && !error && (
                <div className={styles.tableWrap}>
                    <table className={styles.table}>
                        <thead className={styles.thead}>
                            <tr>
                                <th className={styles.th}>Username</th>
                                <th className={styles.th}>Display Name</th>
                                <th className={styles.th}>Email</th>
                                <th className={styles.th}>Status</th>
                                <th className={styles.th}>Role</th>
                                <th className={styles.th}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map(u => (
                                <UserRow key={u.id} user={u} onChanged={refetch} />
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {showCreateModal && (
                <CreateUserModal
                    onClose={() => setShowCreateModal(false)}
                    onCreated={refetch}
                />
            )}
        </div>
    );
}
