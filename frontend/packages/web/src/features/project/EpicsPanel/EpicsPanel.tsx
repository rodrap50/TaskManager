import { useEffect, useState } from 'react';
import { Check, Pencil, Plus, Trash2, X } from 'lucide-react';
import type { EpicDto } from '@taskmanager/shared';
import { createEpic, updateEpic, deleteEpic, getErrorMessage } from '@taskmanager/shared';
import { useEpics } from '../../../hooks/useEpics';
import { COLOR_SWATCHES } from '../../../constants/colorSwatches';
import { useEpicsPanelStyles } from './EpicsPanel.styles';

interface EpicsPanelProps {
    projectId: string;
    projectColorHex?: string | null;
    onClose: () => void;
    onEpicsChanged?: () => void;
}

/** Picks a random swatch from the shared project-color palette, excluding the project's own color. */
function pickRandomEpicColor(excludeHex?: string | null): string {
    const normalizedExclude = excludeHex?.replace('#', '').toUpperCase();
    const pool = COLOR_SWATCHES.filter(c => c.replace('#', '').toUpperCase() !== normalizedExclude);
    const candidates = pool.length > 0 ? pool : COLOR_SWATCHES;
    return candidates[Math.floor(Math.random() * candidates.length)].replace('#', '');
}

interface EpicRowProps {
    epic: EpicDto;
    onRename: (name: string) => Promise<unknown>;
    onDelete: () => Promise<unknown>;
    styles: ReturnType<typeof useEpicsPanelStyles>;
}

function EpicRow({ epic, onRename, onDelete, styles }: EpicRowProps) {
    const [isEditing, setIsEditing] = useState(false);
    const [name, setName] = useState(epic.name);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSaveRename = async () => {
        const trimmed = name.trim();
        if (!trimmed || trimmed === epic.name) {
            setName(epic.name);
            setIsEditing(false);
            return;
        }
        setBusy(true);
        setError(null);
        try {
            await onRename(trimmed);
            setIsEditing(false);
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to rename epic'));
        } finally {
            setBusy(false);
        }
    };

    const handleDelete = async () => {
        setBusy(true);
        setError(null);
        try {
            await onDelete();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to delete epic'));
            setBusy(false);
        }
    };

    return (
        <div className={styles.epicRowWrap}>
            <div className={styles.epicRow}>
                {isEditing ? (
                    <input
                        autoFocus
                        value={name}
                        onChange={e => setName(e.target.value)}
                        onKeyDown={e => {
                            if (e.key === 'Enter') void handleSaveRename();
                            if (e.key === 'Escape') { setName(epic.name); setIsEditing(false); }
                        }}
                        disabled={busy}
                        className={styles.renameInput}
                    />
                ) : (
                    <span className={styles.epicIdentity}>
                        <span
                            className={styles.colorDot}
                            style={{ backgroundColor: epic.colorHex ? `#${epic.colorHex}` : 'transparent' }}
                        />
                        <span className={styles.epicName}>{epic.name}</span>
                    </span>
                )}

                <div className={styles.rowActions}>
                    {isEditing ? (
                        <>
                            <button
                                type="button"
                                disabled={busy}
                                onClick={() => void handleSaveRename()}
                                className={styles.iconButton}
                                aria-label="Save name"
                                title="Save"
                            >
                                <Check className={styles.icon} />
                            </button>
                            <button
                                type="button"
                                disabled={busy}
                                onClick={() => { setName(epic.name); setIsEditing(false); }}
                                className={styles.iconButton}
                                aria-label="Cancel rename"
                                title="Cancel"
                            >
                                <X className={styles.icon} />
                            </button>
                        </>
                    ) : (
                        <>
                            <button
                                type="button"
                                disabled={busy}
                                onClick={() => setIsEditing(true)}
                                className={styles.iconButton}
                                aria-label={`Rename ${epic.name}`}
                                title="Rename"
                            >
                                <Pencil className={styles.icon} />
                            </button>
                            <button
                                type="button"
                                disabled={busy}
                                onClick={() => void handleDelete()}
                                className={styles.deleteButton}
                                aria-label={`Delete ${epic.name}`}
                                title="Delete"
                            >
                                <Trash2 className={styles.icon} />
                            </button>
                        </>
                    )}
                </div>
            </div>
            {error && <p className={styles.rowError}>{error}</p>}
        </div>
    );
}

export function EpicsPanel({ projectId, projectColorHex, onClose, onEpicsChanged }: EpicsPanelProps) {
    const styles = useEpicsPanelStyles();
    const { data: epics, isLoading, refetch } = useEpics(projectId);

    const [showCreate, setShowCreate] = useState(false);
    const [newName, setNewName] = useState('');
    const [creating, setCreating] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);

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

    const handleCreate = async () => {
        const trimmed = newName.trim();
        if (!trimmed) return;
        setCreating(true);
        setCreateError(null);
        try {
            await createEpic({
                projectId,
                name: trimmed,
                displayOrder: epics.length,
                colorHex: pickRandomEpicColor(projectColorHex),
            });
            setNewName('');
            setShowCreate(false);
            await refetch();
            onEpicsChanged?.();
        } catch (e) {
            setCreateError(getErrorMessage(e, 'Failed to create epic'));
        } finally {
            setCreating(false);
        }
    };

    return (
        <>
            <div className={styles.backdrop} onClick={onClose} />
            <div className={styles.panel}>
                <div className={styles.header}>
                    <h2 className={styles.titleText}>Epics</h2>
                    <button type="button" onClick={onClose} className={styles.closeButton} aria-label="Close">
                        <X className={styles.closeIcon} />
                    </button>
                </div>

                <div className={styles.body}>
                    {isLoading && <p className={styles.loadingText}>Loading…</p>}

                    {!isLoading && epics.length === 0 && (
                        <p className={styles.emptyText}>No epics yet.</p>
                    )}

                    {!isLoading && epics.length > 0 && (
                        <div className={styles.epicList}>
                            {epics.map(e => (
                                <EpicRow
                                    key={e.id}
                                    epic={e}
                                    onRename={async (name) => {
                                        await updateEpic(e.id, { name });
                                        await refetch();
                                        onEpicsChanged?.();
                                    }}
                                    onDelete={async () => {
                                        await deleteEpic(e.id);
                                        await refetch();
                                        onEpicsChanged?.();
                                    }}
                                    styles={styles}
                                />
                            ))}
                        </div>
                    )}

                    <div className={styles.addSection}>
                        {!showCreate ? (
                            <button type="button" onClick={() => setShowCreate(true)} className={styles.addToggleButton}>
                                <Plus className={styles.addToggleIcon} />
                                New epic
                            </button>
                        ) : (
                            <>
                                <input
                                    autoFocus
                                    value={newName}
                                    onChange={e => setNewName(e.target.value)}
                                    onKeyDown={e => { if (e.key === 'Enter') void handleCreate(); }}
                                    placeholder="Epic name…"
                                    disabled={creating}
                                    className={styles.createInput}
                                />
                                <div className={styles.createRow}>
                                    <button
                                        type="button"
                                        disabled={creating || !newName.trim()}
                                        onClick={() => void handleCreate()}
                                        className={styles.createButton}
                                    >
                                        {creating ? 'Creating…' : 'Create'}
                                    </button>
                                    <button
                                        type="button"
                                        disabled={creating}
                                        onClick={() => { setShowCreate(false); setNewName(''); setCreateError(null); }}
                                        className={styles.addToggleButton}
                                    >
                                        Cancel
                                    </button>
                                </div>
                                {createError && <p className={styles.createError}>{createError}</p>}
                            </>
                        )}
                    </div>
                </div>
            </div>
        </>
    );
}
