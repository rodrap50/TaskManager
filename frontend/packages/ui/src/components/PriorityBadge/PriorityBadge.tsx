import { usePriorityBadgeStyles } from './PriorityBadge.styles';

interface Props {
    priority: string;
}

const CONFIG: Record<string, { label: string; dot: string }> = {
    Low:      { label: 'Low',      dot: 'bg-text-muted'  },
    Medium:   { label: 'Medium',   dot: 'bg-yellow-500'  },
    High:     { label: 'High',     dot: 'bg-orange-500'  },
    Critical: { label: 'Critical', dot: 'bg-primary-800' },
};

export function PriorityBadge({ priority }: Props) {
    const styles = usePriorityBadgeStyles();
    const cfg = CONFIG[priority] ?? CONFIG.Medium;

    return (
        <span className={styles.badge}>
            <span className={`${styles.dot} ${cfg.dot}`} />
            {cfg.label}
        </span>
    );
}
