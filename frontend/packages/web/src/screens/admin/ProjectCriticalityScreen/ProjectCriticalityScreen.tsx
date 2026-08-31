import { useEffect, useState } from 'react';
import { setProjectCriticalityScore, getErrorMessage, type ProjectDto } from '@taskmanager/shared';
import { useProjects } from '../../../hooks/useProjects';
import { useProjectCriticalityScreenStyles } from './ProjectCriticalityScreen.styles';

interface RowProps {
    project: ProjectDto;
    styles: ReturnType<typeof useProjectCriticalityScreenStyles>;
    onChanged: () => void | Promise<void>;
}

function ProjectRow({ project, styles, onChanged }: RowProps) {
    const [value, setValue] = useState(String(project.criticalityScore));
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        setValue(String(project.criticalityScore));
    }, [project.criticalityScore]);

    const parsed = Number(value);
    const isValid = Number.isInteger(parsed) && parsed >= 1 && parsed <= 10;
    const isDirty = isValid && parsed !== project.criticalityScore;

    const handleSave = async () => {
        if (!isDirty) return;

        setBusy(true);
        setError(null);
        try {
            await setProjectCriticalityScore(project.id, parsed);
            await onChanged();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to update criticality score'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <>
            <tr className={styles.row}>
                <td className={styles.td}>
                    <span className={styles.projectName}>{project.name}</span>
                </td>
                <td className={styles.tdMuted}>{project.scope}</td>
                <td className={styles.td}>
                    <div className={styles.scoreCell}>
                        <input
                            type="number"
                            min={1}
                            max={10}
                            value={value}
                            disabled={busy}
                            onChange={e => setValue(e.target.value)}
                            onKeyDown={e => { if (e.key === 'Enter') void handleSave(); }}
                            className={styles.scoreInput}
                            aria-label={`Criticality score for ${project.name}`}
                        />
                        <button
                            type="button"
                            disabled={!isDirty || busy}
                            onClick={() => void handleSave()}
                            className={styles.saveButton}
                        >
                            {busy ? 'Saving…' : 'Save'}
                        </button>
                    </div>
                </td>
            </tr>
            {error && (
                <tr>
                    <td colSpan={3} className={styles.rowError}>{error}</td>
                </tr>
            )}
        </>
    );
}

/**
 * PRI02.5 — admin-only screen for setting each project's `CriticalityScore` (1–10, folded
 * into every task's weighted priority score). Lives under Admin rather than a per-project
 * panel per user direction. This is still an interim home — see ROADMAP.md's "Project
 * Settings page (per-project, admin-scoped)" backlog item; this screen (or the per-row
 * control) should migrate there once that page exists.
 */
export function ProjectCriticalityScreen() {
    const { data: projects, isLoading, error, refetch } = useProjects();
    const styles = useProjectCriticalityScreenStyles();

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>Project Criticality</h2>
            </div>

            {isLoading && <p className={styles.loadingText}>Loading…</p>}
            {error && <p className={styles.errorText}>Failed to load projects — check the API is running</p>}

            {!isLoading && !error && (
                <div className={styles.tableWrap}>
                    <table className={styles.table}>
                        <thead className={styles.thead}>
                            <tr>
                                <th className={styles.th}>Project</th>
                                <th className={styles.th}>Scope</th>
                                <th className={styles.th}>Criticality (1–10)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {projects.map(p => (
                                <ProjectRow key={p.id} project={p} styles={styles} onChanged={refetch} />
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
