import { useEffect, useMemo, useState } from 'react';
import { UserMinus, UserPlus, X } from 'lucide-react';
import type { AppUserDto } from '@taskmanager/shared';
import { addProjectMember, removeProjectMember, resolveAssetUrl, getErrorMessage } from '@taskmanager/shared';
import { AssigneeAvatar } from '@taskmanager/ui';
import { useProjectMembers } from '../../../hooks/useProjectMembers';
import { useMembersPanelStyles } from './MembersPanel.styles';

interface MembersPanelProps {
    projectId: string;
    users: AppUserDto[];
    usersLoading: boolean;
    onClose: () => void;
    onMembershipChanged?: () => void;
}

interface MemberRowProps {
    member: AppUserDto;
    onRemove: () => Promise<unknown>;
    styles: ReturnType<typeof useMembersPanelStyles>;
}

function MemberRow({ member, onRemove, styles }: MemberRowProps) {
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleRemove = async () => {
        setBusy(true);
        setError(null);
        try {
            await onRemove();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to remove member'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className={styles.memberRowWrap}>
            <div className={styles.memberRow}>
                <div className={styles.memberIdentity}>
                    <AssigneeAvatar displayName={member.displayName} avatarUrl={resolveAssetUrl(member.avatarUrl)} size="sm" />
                    <span className={styles.memberUsername}>{member.username}</span>
                </div>
                <button
                    type="button"
                    disabled={busy}
                    onClick={() => void handleRemove()}
                    className={styles.removeButton}
                    aria-label={`Remove ${member.displayName}`}
                    title="Remove member"
                >
                    <UserMinus className={styles.removeButtonIcon} />
                </button>
            </div>
            {error && <p className={styles.rowError}>{error}</p>}
        </div>
    );
}

export function MembersPanel({ projectId, users, usersLoading, onClose, onMembershipChanged }: MembersPanelProps) {
    const styles = useMembersPanelStyles();
    const { data: members, isLoading: loadingMembers, refetch } = useProjectMembers(projectId);

    const [showAdd, setShowAdd] = useState(false);
    const [filter, setFilter] = useState('');
    const [addingUserId, setAddingUserId] = useState<string | null>(null);
    const [addError, setAddError] = useState<string | null>(null);

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose();
        };
        document.addEventListener('keydown', onKeyDown);

        const prevOverflow = document.body.style.overflow;
        document.body.style.overflow = 'hidden';

        return () => {
            document.removeEventListener('keydown', onKeyDown);
            document.body.style.overflow = prevOverflow;
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const nonMembers = useMemo(() => {
        const memberIds = new Set(members.map(m => m.id));
        const q = filter.trim().toLowerCase();
        return users
            .filter(u => !memberIds.has(u.id))
            .filter(u => !q || u.displayName.toLowerCase().includes(q) || u.username.toLowerCase().includes(q));
    }, [users, members, filter]);

    const handleAdd = async (userId: string) => {
        setAddingUserId(userId);
        setAddError(null);
        try {
            await addProjectMember(projectId, userId);
            await refetch();
            onMembershipChanged?.();
        } catch (e) {
            setAddError(getErrorMessage(e, 'Failed to add member'));
        } finally {
            setAddingUserId(null);
        }
    };

    return (
        <>
            <div className={styles.backdrop} onClick={onClose} />
            <div className={styles.panel}>
                <div className={styles.header}>
                    <h2 className={styles.titleText}>Members</h2>
                    <button type="button" onClick={onClose} className={styles.closeButton} aria-label="Close">
                        <X className={styles.closeIcon} />
                    </button>
                </div>

                <div className={styles.body}>
                    {loadingMembers && <p className={styles.loadingText}>Loading…</p>}

                    {!loadingMembers && members.length === 0 && (
                        <p className={styles.emptyText}>No members yet.</p>
                    )}

                    {!loadingMembers && members.length > 0 && (
                        <div className={styles.memberList}>
                            {members.map(m => (
                                <MemberRow
                                    key={m.id}
                                    member={m}
                                    onRemove={() => removeProjectMember(projectId, m.id).then(async () => {
                                        await refetch();
                                        onMembershipChanged?.();
                                    })}
                                    styles={styles}
                                />
                            ))}
                        </div>
                    )}

                    <div className={styles.addSection}>
                        {!showAdd ? (
                            <button type="button" onClick={() => setShowAdd(true)} className={styles.addToggleButton}>
                                <UserPlus className={styles.addToggleIcon} />
                                Add member
                            </button>
                        ) : (
                            <>
                                <input
                                    autoFocus
                                    value={filter}
                                    onChange={e => setFilter(e.target.value)}
                                    placeholder="Filter users…"
                                    className={styles.filterInput}
                                />
                                {usersLoading && nonMembers.length === 0 && (
                                    <p className={styles.loadingText}>Loading users…</p>
                                )}
                                {!usersLoading && nonMembers.length === 0 && (
                                    <p className={styles.emptyText}>No matching users.</p>
                                )}
                                {nonMembers.length > 0 && (
                                    <div className={styles.addGrid}>
                                        {nonMembers.map(u => (
                                            <button
                                                key={u.id}
                                                type="button"
                                                disabled={addingUserId === u.id}
                                                onClick={() => void handleAdd(u.id)}
                                                className={`${styles.addAvatarButton} ${addingUserId === u.id ? styles.addAvatarBusy : ''}`}
                                                title={u.displayName}
                                            >
                                                <AssigneeAvatar displayName={u.displayName} avatarUrl={resolveAssetUrl(u.avatarUrl)} size="md" />
                                            </button>
                                        ))}
                                    </div>
                                )}
                                {addError && <p className={styles.addError}>{addError}</p>}
                            </>
                        )}
                    </div>
                </div>
            </div>
        </>
    );
}
