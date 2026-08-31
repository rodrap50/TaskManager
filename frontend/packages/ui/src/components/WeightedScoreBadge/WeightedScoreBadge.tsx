import { useWeightedScoreBadgeStyles } from './WeightedScoreBadge.styles';

interface Props {
    score: number;
    className?: string;
}

/**
 * Green → yellow → red as the score (1–10) climbs, so urgency reads at a glance without
 * needing to parse the number. Computed inline (not a token) since it's a continuous
 * gradient over a numeric scale, not a fixed palette choice — same rationale as the
 * per-project/per-epic custom color hex already applied via inline style elsewhere.
 */
function scoreToColor(score: number): string {
    const t = (Math.min(10, Math.max(1, score)) - 1) / 9; // 0 (score=1) .. 1 (score=10)
    const hue = t <= 0.5 ? 120 - (t / 0.5) * 60 : 60 - ((t - 0.5) / 0.5) * 60; // 120=green, 60=yellow, 0=red
    return `hsl(${hue}, 70%, 40%)`;
}

/**
 * Numeric badge for a task's computed weighted priority score (PRI01.2). Position-agnostic —
 * callers place it (e.g. TaskCard's bottom-right corner) via `className` or a wrapping element.
 */
export function WeightedScoreBadge({ score, className }: Props) {
    const styles = useWeightedScoreBadgeStyles();

    return (
        <span
            className={className ? `${styles.badge} ${className}` : styles.badge}
            style={{ backgroundColor: scoreToColor(score) }}
            title={`Weighted score: ${score}`}
        >
            {score}
        </span>
    );
}
