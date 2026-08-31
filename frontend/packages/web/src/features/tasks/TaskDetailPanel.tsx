import { useEffect, useState } from 'react';
import { Pencil, X } from 'lucide-react';
import type { ProjectTaskDto, ProjectPhaseDto, EpicDto, AppUserDto, TaskStatus } from '@taskmanager/shared';
import { transitionTask, updateTask, resolveAssetUrl } from '@taskmanager/shared';
import { AssigneeAvatar } from '@taskmanager/ui';
import { useAuth } from '../../App/AuthContext';
import { useTaskDetailPanelStyles } from './TaskDetailPanel.styles';
import { VoteControl } from './VoteControl';

const STATUS_OPTIONS   = ['Backlog', 'InProgress', 'Blocked', 'Done', 'Cancelled'] as const;
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'] as const;
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

interface TaskDetailPanelProps {
    task: ProjectTaskDto;
    users: AppUserDto[];
    members: AppUserDto[];
    phases: ProjectPhaseDto[];
    epics: EpicDto[];
    projectScope: string;
    onClose: () => void;
    refetch: () => void | Promise<void>;
}

function toDateInputValue(iso: string | null): string {
    return iso ? iso.slice(0, 10) : '';
}

function isOverdue(dateInputValue: string): boolean {
    if (!dateInputValue) return false;
    return dateInputValue < new Date().toISOString().slice(0, 10);
}

function formatDateInputValue(value: string): string {
    if (!value) return 'No due date';
    const [y, m, d] = value.split('-').map(Number);
    return `${MONTHS[m - 1]} ${d}, ${y}`;
}

function snapshotOf(task: ProjectTaskDto) {
    return {
        status: task.status,
        priority: task.priority,
        phaseId: task.phaseId,
        epicId: task.epicId,
        assigneeId: task.assignedUserId,
        dueDate: toDateInputValue(task.dueDate),
        title: task.title,
        description: task.description ?? '',
    };
}

export function TaskDetailPanel({ task, users, members, phases, epics, projectScope, onClose, refetch }: TaskDetailPanelProps) {
    const styles = useTaskDetailPanelStyles();
    const { user } = useAuth();

    const [isEditing, setIsEditing] = useState(false);
    const [saving, setSaving]       = useState(false);
    const [saveError, setSaveError] = useState<string | null>(null);

    // The last-known-persisted values. Seeded from the `task` prop, but re-synced after every
    // successful Save — the prop itself stays stale until the panel closes and BoardScreen refetches,
    // so Cancel/handleSave must compare against this, not `task`, or a post-save Edit->Cancel would
    // silently revert the panel's display to pre-save values even though the server already has the new ones.
    const [saved, setSaved] = useState(() => snapshotOf(task));

    const [status, setStatus]         = useState(saved.status);
    const [priority, setPriority]     = useState(saved.priority);
    const [phaseId, setPhaseId]       = useState(saved.phaseId);
    const [epicId, setEpicId]         = useState(saved.epicId);
    const [assigneeId, setAssigneeId] = useState(saved.assigneeId);
    const [dueDate, setDueDate]       = useState(saved.dueDate);
    const [title, setTitle]           = useState(saved.title);
    const [description, setDescription] = useState(saved.description);

    const handleClose = () => {
        void refetch();
        onClose();
    };

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') handleClose();
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

    const handleDueDateChange = (value: string) => {
        if (!value) {
            // Clearing a due date isn't supported by the API — revert instead of drifting from persisted state.
            setDueDate(saved.dueDate);
            return;
        }
        setDueDate(value);
    };

    const handleCancel = () => {
        setStatus(saved.status);
        setPriority(saved.priority);
        setPhaseId(saved.phaseId);
        setEpicId(saved.epicId);
        setAssigneeId(saved.assigneeId);
        setDueDate(saved.dueDate);
        setTitle(saved.title);
        setDescription(saved.description);
        setSaveError(null);
        setIsEditing(false);
    };

    const handleSave = async () => {
        const trimmedTitle = title.trim();
        if (!trimmedTitle) return;
        const trimmedDescription = description.trim();

        setSaving(true);
        setSaveError(null);
        try {
            if (status !== saved.status) {
                await transitionTask(task.id, status as TaskStatus);
            }
            await updateTask(task.id, {
                title: trimmedTitle,
                description: trimmedDescription,
                priority: priority as typeof PRIORITY_OPTIONS[number],
                ...(phaseId ? { phaseId } : { clearPhase: true }),
                ...(epicId ? { epicId } : { clearEpic: true }),
                ...(assigneeId ? { assignedUserId: assigneeId } : { clearAssignee: true }),
                ...(dueDate ? { dueDate } : {}),
            });
            setTitle(trimmedTitle);
            setDescription(trimmedDescription);
            setSaved({ status, priority, phaseId, epicId, assigneeId, dueDate, title: trimmedTitle, description: trimmedDescription });
            setIsEditing(false);
        } catch (e) {
            setSaveError(e instanceof Error ? e.message : 'Failed to save changes');
        } finally {
            setSaving(false);
        }
    };

    const canSave = title.trim().length > 0 && !saving;
    const phaseName = phaseId ? phases.find(ph => ph.id === phaseId)?.name ?? 'None' : 'None';
    const epicName  = epicId ? epics.find(e => e.id === epicId)?.name ?? 'None' : 'None';
    const showEpics = projectScope === 'Project';
    const showPhases = projectScope !== 'DailyTask';
    const assignee  = assigneeId ? users.find(u => u.id === assigneeId) : undefined;

    return (
        <>
            <div className={styles.backdrop} onClick={handleClose} />
            <div className={styles.panel}>
                <div className={styles.header}>
                    {isEditing ? (
                        <input
                            autoFocus
                            value={title}
                            onChange={e => setTitle(e.target.value)}
                            className={styles.titleInput}
                        />
                    ) : (
                        <h2 className={styles.titleText}>{title}</h2>
                    )}
                    <div className={styles.headerActions}>
                        {!isEditing && (
                            <button type="button" onClick={() => setIsEditing(true)} className={styles.editButton}>
                                <Pencil className={styles.editButtonIcon} />
                                Edit
                            </button>
                        )}
                        <button type="button" onClick={handleClose} className={styles.closeButton} aria-label="Close">
                            <X className={styles.closeIcon} />
                        </button>
                    </div>
                </div>

                <div className={styles.body}>
                    {isEditing ? (
                        <textarea
                            value={description}
                            onChange={e => setDescription(e.target.value)}
                            rows={3}
                            placeholder="Add a description…"
                            className={styles.descriptionInput}
                        />
                    ) : description ? (
                        <p className={styles.descriptionText}>{description}</p>
                    ) : (
                        <p className={styles.descriptionPlaceholder}>No description</p>
                    )}

                    <p className={styles.sectionLabel}>Status</p>
                    {isEditing ? (
                        <div className={styles.pillRow}>
                            {STATUS_OPTIONS.map(s => (
                                <button
                                    key={s}
                                    type="button"
                                    onClick={() => setStatus(s)}
                                    className={`${styles.pillBase} ${status === s ? styles.pillActive : styles.pillInactive}`}
                                >
                                    {s === 'InProgress' ? 'In Progress' : s}
                                </button>
                            ))}
                        </div>
                    ) : (
                        <p className={styles.readOnlyValue}>{status === 'InProgress' ? 'In Progress' : status}</p>
                    )}

                    <p className={styles.sectionLabel}>Priority</p>
                    {isEditing ? (
                        <div className={styles.pillRow}>
                            {PRIORITY_OPTIONS.map(p => (
                                <button
                                    key={p}
                                    type="button"
                                    onClick={() => setPriority(p)}
                                    className={`${styles.pillBase} ${priority === p ? styles.pillActive : styles.pillInactive}`}
                                >
                                    {p}
                                </button>
                            ))}
                        </div>
                    ) : (
                        <p className={styles.readOnlyValue}>{priority}</p>
                    )}

                    {user && <VoteControl task={task} currentUserId={user.id} />}

                    {showPhases && phases.length > 0 && (
                        <>
                            <p className={styles.sectionLabel}>Phase</p>
                            {isEditing ? (
                                <div className={styles.pillRow}>
                                    <button
                                        type="button"
                                        onClick={() => setPhaseId(null)}
                                        className={`${styles.pillBase} ${!phaseId ? styles.pillActive : styles.pillInactive}`}
                                    >
                                        None
                                    </button>
                                    {phases.map(ph => (
                                        <button
                                            key={ph.id}
                                            type="button"
                                            onClick={() => setPhaseId(ph.id)}
                                            className={`${styles.pillBase} ${phaseId === ph.id ? styles.pillActive : styles.pillInactive}`}
                                        >
                                            {ph.name}
                                        </button>
                                    ))}
                                </div>
                            ) : (
                                <p className={styles.readOnlyValue}>{phaseName}</p>
                            )}
                        </>
                    )}

                    {showEpics && epics.length > 0 && (
                        <>
                            <p className={styles.sectionLabel}>Epic</p>
                            {isEditing ? (
                                <div className={styles.pillRow}>
                                    <button
                                        type="button"
                                        onClick={() => setEpicId(null)}
                                        className={`${styles.pillBase} ${!epicId ? styles.pillActive : styles.pillInactive}`}
                                    >
                                        None
                                    </button>
                                    {epics.map(e => (
                                        <button
                                            key={e.id}
                                            type="button"
                                            onClick={() => setEpicId(e.id)}
                                            className={`${styles.pillBase} ${epicId === e.id ? styles.pillActive : styles.pillInactive}`}
                                        >
                                            {e.name}
                                        </button>
                                    ))}
                                </div>
                            ) : (
                                <p className={styles.readOnlyValue}>{epicName}</p>
                            )}
                        </>
                    )}

                    {users.length > 0 && (
                        <>
                            <p className={styles.sectionLabel}>Assignee</p>
                            {isEditing ? (
                                <div className={styles.assigneeRow}>
                                    <button
                                        type="button"
                                        onClick={() => setAssigneeId(null)}
                                        className={`${styles.assigneeButton} ${!assigneeId ? styles.assigneeSelected : ''}`}
                                    >
                                        <span className={styles.unassignedAvatar}>—</span>
                                    </button>
                                    {members.map(u => (
                                        <button
                                            key={u.id}
                                            type="button"
                                            onClick={() => setAssigneeId(assigneeId === u.id ? null : u.id)}
                                            className={`${styles.assigneeButton} ${assigneeId === u.id ? styles.assigneeSelected : ''}`}
                                        >
                                            <AssigneeAvatar displayName={u.displayName} avatarUrl={resolveAssetUrl(u.avatarUrl)} size="md" />
                                        </button>
                                    ))}
                                </div>
                            ) : assignee ? (
                                <div className={styles.readOnlyAssignee}>
                                    <AssigneeAvatar displayName={assignee.displayName} avatarUrl={resolveAssetUrl(assignee.avatarUrl)} size="sm" />
                                    <span className={styles.readOnlyValue}>{assignee.displayName}</span>
                                </div>
                            ) : (
                                <p className={styles.readOnlyValue}>Unassigned</p>
                            )}
                        </>
                    )}

                    <p className={styles.sectionLabel}>Due date</p>
                    {isEditing ? (
                        <input
                            type="date"
                            value={dueDate}
                            onChange={e => handleDueDateChange(e.target.value)}
                            className={styles.dateInput}
                        />
                    ) : (
                        <p className={isOverdue(dueDate) ? styles.dateOverdue : styles.readOnlyValue}>
                            {formatDateInputValue(dueDate)}
                        </p>
                    )}
                </div>

                {isEditing && (
                    <div className={styles.footer}>
                        {saveError && <p className={styles.fieldError}>{saveError}</p>}
                        <div className={styles.footerButtons}>
                            <button type="button" onClick={handleCancel} className={styles.cancelButton}>
                                Cancel
                            </button>
                            <button type="button" onClick={() => void handleSave()} disabled={!canSave} className={styles.saveButton}>
                                {saving ? 'Saving…' : 'Save'}
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </>
    );
}
