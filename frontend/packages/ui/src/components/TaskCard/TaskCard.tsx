import type { ProjectTaskDto } from '@taskmanager/shared';
import { PriorityBadge } from '../PriorityBadge';
import { AssigneeAvatar } from '../AssigneeAvatar';
import { WeightedScoreBadge } from '../WeightedScoreBadge';
import { useTaskCardStyles } from './TaskCard.styles';

interface Props {
    task: ProjectTaskDto;
    epicName?: string;
    epicColorHex?: string | null;
    assigneeName?: string;
    assigneeAvatarUrl?: string | null;
    onClick?: () => void;
}

function formatDue(iso: string | null): string | null {
    if (!iso) return null;
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

function isOverdue(iso: string | null): boolean {
    if (!iso) return false;
    return new Date(iso) < new Date();
}

export function TaskCard({ task, epicName, epicColorHex, assigneeName, assigneeAvatarUrl, onClick }: Props) {
    const styles = useTaskCardStyles();
    const dueLabel = formatDue(task.dueDate);
    const overdue = isOverdue(task.dueDate);

    return (
        <div onClick={onClick} className={styles.card}>
            {epicName && (
                <span
                    className={styles.epicTag}
                    style={epicColorHex ? { backgroundColor: `#${epicColorHex}` } : undefined}
                >
                    {epicName}
                </span>
            )}

            <PriorityBadge priority={task.priority} />

            <p className={styles.title}>{task.title}</p>

            <div className={styles.footer}>
                {dueLabel ? (
                    <span className={overdue ? styles.dueOverdue : styles.dueNormal}>
                        {dueLabel}
                    </span>
                ) : <span />}
                <div className={styles.footerRight}>
                    {assigneeName && (
                        <AssigneeAvatar displayName={assigneeName} avatarUrl={assigneeAvatarUrl} />
                    )}
                    <WeightedScoreBadge score={task.weightedScore} />
                </div>
            </div>
        </div>
    );
}
