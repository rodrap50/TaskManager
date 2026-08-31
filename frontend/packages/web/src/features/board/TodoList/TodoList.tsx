import { useEffect, useState } from 'react';
import { Check, ListChecks } from 'lucide-react';
import type { ProjectTaskDto } from '@taskmanager/shared';
import { transitionTask, getErrorMessage } from '@taskmanager/shared';
import { PriorityBadge, WeightedScoreBadge } from '@taskmanager/ui';
import { useTodoListStyles } from './TodoList.styles';

function formatDue(iso: string | null): { label: string; overdue: boolean } | null {
    if (!iso) return null;
    const due  = new Date(iso);
    const diff = Math.ceil((due.getTime() - Date.now()) / 86_400_000);
    return {
        label:   diff === 0 ? 'Today' : diff === 1 ? 'Tomorrow' : diff < 0 ? `${Math.abs(diff)}d ago` : `${diff}d`,
        overdue: diff < 0,
    };
}

interface RowProps {
    task: ProjectTaskDto;
    onToggle: (taskId: string, done: boolean) => void;
    onClick: () => void;
}

function TodoRow({ task, onToggle, onClick }: RowProps) {
    const styles = useTodoListStyles();
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const done = task.status === 'Done';
    const due = formatDue(task.dueDate);

    const handleToggle = async (e: React.MouseEvent) => {
        e.stopPropagation();
        setBusy(true);
        setError(null);
        try {
            await transitionTask(task.id, done ? 'Backlog' : 'Done');
            onToggle(task.id, !done);
        } catch (err) {
            setError(getErrorMessage(err, 'Failed to update task'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <div onClick={onClick} className={styles.rowWrap}>
            <div className={styles.row}>
                <button
                    type="button"
                    onClick={e => void handleToggle(e)}
                    disabled={busy}
                    aria-label={done ? `Mark "${task.title}" incomplete` : `Mark "${task.title}" complete`}
                    className={`${styles.checkbox} ${done ? styles.checkboxChecked : ''}`}
                >
                    {done && <Check className={styles.checkIcon} />}
                </button>
                <span className={done ? styles.titleDone : styles.title}>{task.title}</span>
                <div className={styles.metaRow}>
                    <PriorityBadge priority={task.priority} />
                    <WeightedScoreBadge score={task.weightedScore} />
                    {due && (
                        <span className={due.overdue ? styles.dueOverdue : styles.dueNormal}>
                            {due.label}
                        </span>
                    )}
                </div>
            </div>
            {error && <p className={styles.rowError}>{error}</p>}
        </div>
    );
}

interface Props {
    tasks: ProjectTaskDto[];
    onTaskClick?: (task: ProjectTaskDto) => void;
}

export function TodoList({ tasks, onTaskClick }: Props) {
    const styles = useTodoListStyles();
    const [localTasks, setLocalTasks] = useState<ProjectTaskDto[]>(tasks);

    useEffect(() => {
        setLocalTasks(tasks);
    }, [tasks]);

    const handleToggle = (taskId: string, done: boolean) => {
        setLocalTasks(prev =>
            prev.map(t => t.id === taskId ? { ...t, status: done ? 'Done' : 'Backlog' } : t)
        );
    };

    if (localTasks.length === 0) {
        return (
            <div className={styles.emptyState}>
                <ListChecks className={styles.emptyIcon} />
                <p className={styles.emptyTitle}>No tasks yet</p>
                <p className={styles.emptyHint}>Create your first task to get started</p>
            </div>
        );
    }

    const sorted = [...localTasks].sort((a, b) => b.weightedScore - a.weightedScore);

    return (
        <div className={styles.container}>
            {sorted.map(task => (
                <TodoRow
                    key={task.id}
                    task={task}
                    onToggle={handleToggle}
                    onClick={() => onTaskClick?.(task)}
                />
            ))}
        </div>
    );
}
