import { useState } from 'react';
import { createProject } from '@taskmanager/shared';
import { useProjects } from '../../../hooks/useProjects';
import { useProject } from '../../../contexts/ProjectContext';
import { useCreateProjectModalStyles } from './CreateProjectModal.styles';
import { COLOR_SWATCHES } from '../../../constants/colorSwatches';

const SCOPE_OPTIONS = ['Project', 'DailyTask'] as const;
type Scope = typeof SCOPE_OPTIONS[number];

export function CreateProjectModal({ onClose }: { onClose: () => void }) {
    const { refetch } = useProjects();
    const { selectProject } = useProject();
    const styles = useCreateProjectModalStyles();

    const [name, setName]             = useState('');
    const [description, setDesc]      = useState('');
    const [scope, setScope]           = useState<Scope>('Project');
    const [colorHex, setColor]        = useState(COLOR_SWATCHES[0]);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async () => {
        if (!name.trim()) return;
        setSubmitting(true);
        setError(null);
        try {
            const project = await createProject({
                name: name.trim(),
                description: description.trim() || undefined,
                scope,
                colorHex: colorHex.replace('#', ''),
            });
            await refetch();
            selectProject(project.id);
            onClose();
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to create project');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div
            className={styles.overlay}
            onClick={e => e.target === e.currentTarget && onClose()}
        >
            <div className={styles.panel}>
                <h2 className={styles.heading}>New Project</h2>

                <label className={styles.label}>Name</label>
                <input
                    autoFocus
                    value={name}
                    onChange={e => setName(e.target.value)}
                    onKeyDown={e => e.key === 'Enter' && void handleSubmit()}
                    placeholder="Project name"
                    className={styles.input}
                />

                <label className={styles.label}>Description</label>
                <textarea
                    value={description}
                    onChange={e => setDesc(e.target.value)}
                    rows={2}
                    placeholder="Optional"
                    className={styles.textarea}
                />

                <label className={styles.label}>Type</label>
                <div className={styles.typeRow}>
                    {SCOPE_OPTIONS.map(s => (
                        <button
                            key={s}
                            onClick={() => setScope(s)}
                            className={`${styles.typeButtonBase} ${scope === s ? styles.typeButtonActive : styles.typeButtonInactive}`}
                        >
                            {s === 'DailyTask' ? 'Daily' : s}
                        </button>
                    ))}
                </div>

                <label className={styles.label}>Colour</label>
                <div className={styles.colorRow}>
                    {COLOR_SWATCHES.map(c => (
                        <button
                            key={c}
                            type="button"
                            onClick={() => setColor(c)}
                            className={`${styles.colorSwatchBase} ${colorHex === c ? styles.colorSwatchActive : styles.colorSwatchInactive}`}
                        >
                            <span className={styles.colorSwatchFill} style={{ backgroundColor: c }} />
                        </button>
                    ))}
                </div>

                {error && (
                    <div className={styles.errorBox}>
                        {error}
                    </div>
                )}

                <div className={styles.actionsRow}>
                    <button onClick={onClose} className={styles.cancelButton}>
                        Cancel
                    </button>
                    <button
                        onClick={() => void handleSubmit()}
                        disabled={!name.trim() || submitting}
                        className={styles.submitButton}
                    >
                        {submitting ? 'Creating…' : 'Create'}
                    </button>
                </div>
            </div>
        </div>
    );
}
