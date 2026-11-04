import { useCallback, useEffect, useState } from 'react';
import { ArchiveRestore, X } from 'lucide-react';
import {
    archiveCompletedTasks, getErrorMessage, getTasksByProject, restoreTask, type ProjectTaskDto,
} from '@taskmanager/shared';
import { useArchivedPanelStyles } from './ArchivedPanel.styles';

interface ArchivedPanelProps {
    projectId: string;
    /** Age, in days, used by the "archive old done tasks" action. */
    olderThanDays: number;
    onClose: () => void;
    onTasksChanged?: () => void | Promise<void>;
}

function formatDate(iso: string | null): string {
    return iso ? new Date(iso).toLocaleDateString() : '';
}

export function ArchivedPanel({ projectId, olderThanDays, onClose, onTasksChanged }: ArchivedPanelProps) {
    const styles = useArchivedPanelStyles();
    const [tasks, setTasks] = useState<ProjectTaskDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [search, setSearch] = useState('');
    const [busyId, setBusyId] = useState<string | null>(null);
    const [archiving, setArchiving] = useState(false);
    const [archiveResult, setArchiveResult] = useState<string | null>(null);

    const load = useCallback(async () => {
        setError(null);
        try {
            setTasks(await getTasksByProject(projectId, { archived: true }));
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to load archived tasks'));
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose();
        };
        document.addEventListener('keydown', onKeyDown);

        const prevOverflow = document.body.style.overflow;
        document.body.style.overflow = 'hidden';

        return () => {
            document.removeEventListener('keydown', onKeyDown);
            document.body.style.overflow = prevOverflow;
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleRestore = async (task: ProjectTaskDto) => {
        setBusyId(task.id);
        setError(null);
        try {
            await restoreTask(task.id);
            await load();
            await onTasksChanged?.();
        } catch (e) {
            setError(getErrorMessage(e, `Failed to restore "${task.title}"`));
        } finally {
            setBusyId(null);
        }
    };

    const handleArchiveOld = async () => {
        setArchiving(true);
        setError(null);
        setArchiveResult(null);
        try {
            const { archivedCount } = await archiveCompletedTasks(projectId, olderThanDays);
            setArchiveResult(archivedCount === 1 ? 'Archived 1 task.' : `Archived ${archivedCount} tasks.`);
            await load();
            await onTasksChanged?.();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to archive tasks'));
        } finally {
            setArchiving(false);
        }
    };

    const query = search.trim().toLowerCase();
    const shown = tasks
        .filter(t => !query || t.title.toLowerCase().includes(query) || (t.description ?? '').toLowerCase().includes(query))
        .sort((a, b) => (b.archivedAt ?? '').localeCompare(a.archivedAt ?? ''));

    return (
        <>
            <div className={styles.backdrop} onClick={onClose} />
            <div className={styles.panel} role="dialog" aria-label="Archived tasks">
                <div className={styles.header}>
                    <h2 className={styles.titleText}>Archived tasks</h2>
                    <button type="button" onClick={onClose} className={styles.closeButton} aria-label="Close">
                        <X className={styles.closeIcon} />
                    </button>
                </div>

                <div className={styles.body}>
                    <div className={styles.archiveSection}>
                        <button
                            type="button"
                            onClick={() => void handleArchiveOld()}
                            disabled={archiving}
                            className={styles.archiveButton}
                        >
                            {archiving ? 'Archiving…' : `Archive tasks done more than ${olderThanDays} days ago`}
                        </button>
                        <p className={styles.hint}>Also archives Cancelled tasks closed before then.</p>
                        {archiveResult && <p className={styles.hint}>{archiveResult}</p>}
                    </div>

                    <input
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        placeholder="Search archived tasks…"
                        aria-label="Search archived tasks"
                        className={styles.searchInput}
                    />

                    {error && <p className={styles.errorText}>{error}</p>}
                    {isLoading && <p className={styles.mutedText}>Loading…</p>}
                    {!isLoading && tasks.length === 0 && <p className={styles.mutedText}>No archived tasks.</p>}
                    {!isLoading && tasks.length > 0 && shown.length === 0 && (
                        <p className={styles.mutedText}>No archived tasks match "{search.trim()}".</p>
                    )}

                    <ul className={styles.list}>
                        {shown.map(t => (
                            <li key={t.id} className={styles.row}>
                                <div className={styles.rowText}>
                                    <p className={styles.rowTitle}>{t.title}</p>
                                    <p className={styles.rowMeta}>
                                        {t.status} · archived {formatDate(t.archivedAt)}
                                    </p>
                                </div>
                                <button
                                    type="button"
                                    onClick={() => void handleRestore(t)}
                                    disabled={busyId === t.id}
                                    className={styles.restoreButton}
                                    aria-label={`Restore ${t.title}`}
                                    title="Restore to the board"
                                >
                                    <ArchiveRestore className={styles.icon} />
                                </button>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>
        </>
    );
}
