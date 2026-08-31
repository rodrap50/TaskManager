import { useEffect, useState } from 'react';
import { X } from 'lucide-react';
import type { ProjectTaskDto, ProjectPhaseDto, EpicDto, AppUserDto, TaskStatus } from '@taskmanager/shared';
import { transitionTask, resolveAssetUrl } from '@taskmanager/shared';
import { TaskCard } from '@taskmanager/ui';
import { useKanbanBoardStyles } from './KanbanBoard.styles';

type Status = 'Backlog' | 'InProgress' | 'Blocked' | 'Done';

interface Column {
    status: Status;
    label: string;
}

const COLUMNS: Column[] = [
    { status: 'Backlog',    label: 'Backlog'     },
    { status: 'InProgress', label: 'In Progress' },
    { status: 'Blocked',    label: 'Blocked'     },
    { status: 'Done',       label: 'Done'        },
];

interface Props {
    tasks: ProjectTaskDto[];
    phases: ProjectPhaseDto[];
    epics: EpicDto[];
    users: AppUserDto[];
    onTaskClick?: (task: ProjectTaskDto) => void;
}

export function KanbanBoard({ tasks, epics, users, onTaskClick }: Props) {
    const styles = useKanbanBoardStyles();
    const [localTasks, setLocalTasks] = useState<ProjectTaskDto[]>(tasks);
    const [draggedId, setDraggedId]   = useState<string | null>(null);
    const [overCol, setOverCol]       = useState<Status | null>(null);
    const [toast, setToast]           = useState<string | null>(null);

    useEffect(() => {
        setLocalTasks(tasks);
    }, [tasks]);

    useEffect(() => {
        if (!toast) return;
        const t = setTimeout(() => setToast(null), 15000);
        return () => clearTimeout(t);
    }, [toast]);

    const boardTasks = localTasks.filter(t => t.status !== 'Cancelled');

    const userMap = Object.fromEntries(users.map(u => [u.id, u]));
    const epicMap = Object.fromEntries(epics.map(e => [e.id, e]));

    // Done/Cancelled tasks can only reopen to Backlog (enforced server-side by
    // TaskItem.Transition) — while dragging one, every other column is a no-op target.
    const draggedTask       = draggedId ? localTasks.find(t => t.id === draggedId) : undefined;
    const restrictedToBacklog = draggedTask?.status === 'Done' || draggedTask?.status === 'Cancelled';
    const isLockedOut = (status: Status) => restrictedToBacklog && status !== 'Backlog';

    const onDragStart = (e: React.DragEvent, taskId: string) => {
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', taskId);
        setDraggedId(taskId);
    };
    const onDragOver  = (e: React.DragEvent, status: Status) => {
        if (isLockedOut(status)) return; // no preventDefault → browser shows a disallowed drop
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        setOverCol(status);
    };
    const onDrop = async (e: React.DragEvent, targetStatus: Status) => {
        e.preventDefault();
        if (!draggedId || isLockedOut(targetStatus)) return;

        const task = localTasks.find(t => t.id === draggedId);
        if (!task || task.status === targetStatus) {
            setDraggedId(null);
            setOverCol(null);
            return;
        }

        setLocalTasks(prev =>
            prev.map(t => t.id === draggedId ? { ...t, status: targetStatus } : t)
        );
        setDraggedId(null);
        setOverCol(null);

        try {
            await transitionTask(draggedId, targetStatus as TaskStatus);
        } catch {
            setLocalTasks(prev =>
                prev.map(t => t.id === draggedId ? { ...t, status: task.status } : t)
            );
            setToast(`Couldn't move "${task.title}" — try again.`);
        }
    };
    const onDragEnd = () => {
        setDraggedId(null);
        setOverCol(null);
    };

    return (
        <div className={styles.board}>
            {COLUMNS.map(col => {
                const colTasks = boardTasks
                    .filter(t => t.status === col.status)
                    .sort((a, b) => b.weightedScore - a.weightedScore);
                const isOver    = overCol === col.status;
                const isInvalid = draggedId !== null && isLockedOut(col.status);
                return (
                    <div
                        key={col.status}
                        onDragOver={e => onDragOver(e, col.status)}
                        onDrop={e => void onDrop(e, col.status)}
                        className={`${styles.columnBase} ${isInvalid ? styles.columnInvalid : isOver ? styles.columnOver : styles.columnIdle}`}
                    >
                        <div className={styles.header}>
                            <h3 className={styles.headerLabel}>{col.label}</h3>
                            <span className={styles.countBadge}>
                                {colTasks.length}
                            </span>
                        </div>

                        <div className={styles.taskList}>
                            {colTasks.map(task => {
                                const assignee = task.assignedUserId ? userMap[task.assignedUserId] : undefined;
                                return (
                                    <div
                                        key={task.id}
                                        draggable
                                        onDragStart={e => onDragStart(e, task.id)}
                                        onDragEnd={onDragEnd}
                                        className={`${styles.taskWrapBase} ${draggedId === task.id ? styles.taskDragging : styles.taskIdle}`}
                                    >
                                        <TaskCard
                                            task={task}
                                            epicName={task.epicId ? epicMap[task.epicId]?.name : undefined}
                                            epicColorHex={task.epicId ? epicMap[task.epicId]?.colorHex : undefined}
                                            assigneeName={assignee?.displayName}
                                            assigneeAvatarUrl={resolveAssetUrl(assignee?.avatarUrl)}
                                            onClick={() => onTaskClick?.(task)}
                                        />
                                    </div>
                                );
                            })}

                            {colTasks.length === 0 && (
                                <div className={`${styles.emptyBase} ${isOver ? styles.emptyOver : styles.emptyIdle}`}>
                                    No tasks
                                </div>
                            )}
                        </div>
                    </div>
                );
            })}

            {toast && (
                <div className={styles.toast}>
                    <span className={styles.toastTitle}>Error</span>
                    <span className={styles.toastDivider} />
                    <span className={styles.toastMessage}>{toast}</span>
                    <button
                        type="button"
                        onClick={() => setToast(null)}
                        aria-label="Dismiss"
                        className={styles.toastClose}
                    >
                        <X className={styles.toastCloseIcon} />
                    </button>
                </div>
            )}
        </div>
    );
}
