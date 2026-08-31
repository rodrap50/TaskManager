import { LayoutGrid, List } from 'lucide-react';
import { useViewToggleStyles } from './ViewToggle.styles';

export type ViewMode = 'kanban' | 'list';

interface ViewToggleProps {
    mode: ViewMode;
    onChange: (mode: ViewMode) => void;
}

export function ViewToggle({ mode, onChange }: ViewToggleProps) {
    const styles = useViewToggleStyles();

    return (
        <div className={styles.container}>
            <button
                type="button"
                onClick={() => onChange('kanban')}
                title="Kanban view"
                className={`${styles.buttonBase} ${mode === 'kanban' ? styles.buttonActive : styles.buttonInactive}`}
            >
                <LayoutGrid className={styles.icon} />
            </button>
            <button
                type="button"
                onClick={() => onChange('list')}
                title="List view"
                className={`${styles.buttonBase} ${styles.divider} ${mode === 'list' ? styles.buttonActive : styles.buttonInactive}`}
            >
                <List className={styles.icon} />
            </button>
        </div>
    );
}
