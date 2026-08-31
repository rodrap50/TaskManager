import { useState } from 'react';
import type { FormEvent } from 'react';
import type { AppUserDto } from '@taskmanager/shared';
import { resolveAssetUrl } from '@taskmanager/shared';
import { AssigneeAvatar } from '@taskmanager/ui';
import { useTaskFormStyles } from './TaskForm.styles';

const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'] as const;
type Priority = typeof PRIORITY_OPTIONS[number];

export interface TaskFormValues {
    title: string;
    priority: Priority;
    dueDate: string | null;
    assigneeId: string | null;
}

interface TaskFormProps {
    members: AppUserDto[];
    onSubmit: (values: TaskFormValues) => void;
    onCancel: () => void;
    submitting: boolean;
    error: string | null;
}

export function TaskForm({ members, onSubmit, onCancel, submitting, error }: TaskFormProps) {
    const styles = useTaskFormStyles();

    const [title, setTitle]         = useState('');
    const [priority, setPriority]   = useState<Priority>('Medium');
    const [dueDate, setDueDate]     = useState('');
    const [assigneeId, setAssignee] = useState<string | null>(null);

    const canSubmit = title.trim().length > 0 && !submitting;

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        if (!canSubmit) return;
        onSubmit({ title: title.trim(), priority, dueDate: dueDate || null, assigneeId });
    };

    return (
        <form onSubmit={handleSubmit}>
            <label className={styles.label}>Title</label>
            <input
                autoFocus
                value={title}
                onChange={e => setTitle(e.target.value)}
                maxLength={300}
                required
                placeholder="Task title"
                className={styles.input}
            />

            <label className={styles.label}>Priority</label>
            <div className={styles.priorityRow}>
                {PRIORITY_OPTIONS.map(p => (
                    <button
                        key={p}
                        type="button"
                        onClick={() => setPriority(p)}
                        className={`${styles.priorityPillBase} ${p === priority ? styles.priorityPillActive : styles.priorityPillInactive}`}
                    >
                        {p}
                    </button>
                ))}
            </div>

            <label className={styles.label}>Due date</label>
            <div className={styles.dueDateWrap}>
                <div className={styles.dueDateRow}>
                    <input
                        type="date"
                        value={dueDate}
                        onChange={e => setDueDate(e.target.value)}
                        className={styles.dateInput}
                    />
                    {dueDate && (
                        <button type="button" onClick={() => setDueDate('')} className={styles.clearDateButton}>
                            Clear
                        </button>
                    )}
                </div>
                {!dueDate && <p className={styles.noDueDateHint}>No due date</p>}
            </div>

            {members.length > 0 && (
                <>
                    <label className={styles.label}>Assignee</label>
                    <div className={styles.assigneeRow}>
                        <button
                            type="button"
                            onClick={() => setAssignee(null)}
                            className={`${styles.assigneeButton} ${assigneeId === null ? styles.assigneeSelected : ''}`}
                        >
                            <span className={styles.unassignedAvatar}>—</span>
                        </button>
                        {members.map(u => (
                            <button
                                key={u.id}
                                type="button"
                                onClick={() => setAssignee(prev => prev === u.id ? null : u.id)}
                                className={`${styles.assigneeButton} ${assigneeId === u.id ? styles.assigneeSelected : ''}`}
                            >
                                <AssigneeAvatar displayName={u.displayName} avatarUrl={resolveAssetUrl(u.avatarUrl)} size="md" />
                            </button>
                        ))}
                    </div>
                </>
            )}

            {error && <div className={styles.errorBox}>{error}</div>}

            <div className={styles.actionsRow}>
                <button type="button" onClick={onCancel} className={styles.cancelButton}>
                    Cancel
                </button>
                <button type="submit" disabled={!canSubmit} className={styles.submitButton}>
                    {submitting ? 'Creating…' : 'Create Task'}
                </button>
            </div>
        </form>
    );
}
