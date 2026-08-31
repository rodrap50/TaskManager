import { ListChecks } from 'lucide-react';
import type { ProjectTaskDto, ProjectPhaseDto, EpicDto, AppUserDto } from '@taskmanager/shared';
import { resolveAssetUrl } from '@taskmanager/shared';
import { PriorityBadge, StatusIndicator, AssigneeAvatar, WeightedScoreBadge } from '@taskmanager/ui';
import { useListViewStyles } from './ListView.styles';

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
    phaseName?: string;
    epicName?: string;
    epicColorHex?: string | null;
    assignee?: AppUserDto;
    onClick: () => void;
}

function TaskRow({ task, phaseName, epicName, epicColorHex, assignee, onClick }: RowProps) {
    const styles = useListViewStyles();
    const due = formatDue(task.dueDate);
    return (
        <button type="button" onClick={onClick} className={styles.row}>
            <StatusIndicator status={task.status} />
            <span className={styles.title}>{task.title}</span>
            <div className={styles.metaRow}>
                <PriorityBadge priority={task.priority} />
                <WeightedScoreBadge score={task.weightedScore} />
                {phaseName && <span className={styles.phaseTag}>{phaseName}</span>}
                {epicName && (
                    <span
                        className={styles.epicTag}
                        style={epicColorHex ? { backgroundColor: `#${epicColorHex}` } : undefined}
                    >
                        {epicName}
                    </span>
                )}
                {due && (
                    <span className={due.overdue ? styles.dueOverdue : styles.dueNormal}>
                        {due.label}
                    </span>
                )}
                {assignee && (
                    <AssigneeAvatar displayName={assignee.displayName} avatarUrl={resolveAssetUrl(assignee.avatarUrl)} size="sm" />
                )}
            </div>
        </button>
    );
}

interface Props {
    tasks: ProjectTaskDto[];
    phases: ProjectPhaseDto[];
    epics: EpicDto[];
    users: AppUserDto[];
    onTaskClick?: (task: ProjectTaskDto) => void;
}

export function ListView({ tasks, phases, epics, users, onTaskClick }: Props) {
    const styles = useListViewStyles();
    const phaseMap = Object.fromEntries(phases.map(p => [p.id, p]));
    const epicMap  = Object.fromEntries(epics.map(e => [e.id, e]));
    const userMap  = Object.fromEntries(users.map(u => [u.id, u]));

    const sorted = [...tasks].sort((a, b) => {
        const sd = b.weightedScore - a.weightedScore;
        if (sd !== 0) return sd;
        if (!a.dueDate && !b.dueDate) return 0;
        if (!a.dueDate) return 1;
        if (!b.dueDate) return -1;
        return a.dueDate.localeCompare(b.dueDate);
    });

    if (sorted.length === 0) {
        return (
            <div className={styles.emptyState}>
                <ListChecks className={styles.emptyIcon} />
                <p className={styles.emptyTitle}>No tasks yet</p>
                <p className={styles.emptyHint}>Create your first task to get started</p>
            </div>
        );
    }

    return (
        <div className={styles.container}>
            {sorted.map(task => (
                <TaskRow
                    key={task.id}
                    task={task}
                    phaseName={task.phaseId ? phaseMap[task.phaseId]?.name : undefined}
                    epicName={task.epicId ? epicMap[task.epicId]?.name : undefined}
                    epicColorHex={task.epicId ? epicMap[task.epicId]?.colorHex : undefined}
                    assignee={task.assignedUserId ? userMap[task.assignedUserId] : undefined}
                    onClick={() => onTaskClick?.(task)}
                />
            ))}
        </div>
    );
}
