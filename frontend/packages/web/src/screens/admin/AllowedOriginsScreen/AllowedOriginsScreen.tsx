import { useState } from 'react';
import { Trash2 } from 'lucide-react';
import { addAllowedOrigin, removeAllowedOrigin, getErrorMessage, type AllowedOriginDto } from '@taskmanager/shared';
import { useAllowedOrigins } from '../../../hooks/useAllowedOrigins';
import { useAllowedOriginsScreenStyles } from './AllowedOriginsScreen.styles';

interface RowProps {
    origin: AllowedOriginDto;
    styles: ReturnType<typeof useAllowedOriginsScreenStyles>;
    onRemoved: () => void | Promise<void>;
}

function OriginRow({ origin, styles, onRemoved }: RowProps) {
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleRemove = async () => {
        setBusy(true);
        setError(null);
        try {
            await removeAllowedOrigin(origin.id);
            await onRemoved();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to remove origin'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <>
            <tr className={styles.row}>
                <td className={styles.td}>
                    <span className={styles.originUrl}>{origin.originUrl}</span>
                </td>
                <td className={styles.tdMuted}>{new Date(origin.createdAt).toLocaleDateString()}</td>
                <td className={styles.td}>
                    <button
                        type="button"
                        disabled={busy}
                        onClick={() => void handleRemove()}
                        className={styles.removeButton}
                        aria-label={`Remove ${origin.originUrl}`}
                        title="Remove origin"
                    >
                        <Trash2 className={styles.removeButtonIcon} />
                    </button>
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
 * PREP.1 — admin-only screen for managing the CORS allow-list. DB-backed and
 * admin-editable at runtime: adding/removing an origin here takes effect immediately,
 * no API redeploy. Only governs browser callers — server-to-server callers (webhooks,
 * service accounts) are never subject to CORS and authenticate via ApiToken instead.
 */
export function AllowedOriginsScreen() {
    const { data: origins, isLoading, error, refetch } = useAllowedOrigins();
    const styles = useAllowedOriginsScreenStyles();

    const [newOrigin, setNewOrigin] = useState('');
    const [addBusy, setAddBusy] = useState(false);
    const [addError, setAddError] = useState<string | null>(null);

    const handleAdd = async () => {
        if (!newOrigin.trim()) return;

        setAddBusy(true);
        setAddError(null);
        try {
            await addAllowedOrigin(newOrigin.trim());
            setNewOrigin('');
            await refetch();
        } catch (e) {
            setAddError(getErrorMessage(e, 'Failed to add origin'));
        } finally {
            setAddBusy(false);
        }
    };

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>Allowed Origins</h2>
            </div>

            {isLoading && <p className={styles.loadingText}>Loading…</p>}
            {error && <p className={styles.errorText}>Failed to load allowed origins — check the API is running</p>}

            {!isLoading && !error && (
                <div className={styles.tableWrap}>
                    {origins.length === 0 ? (
                        <p className={styles.emptyText}>No allowed origins yet.</p>
                    ) : (
                        <table className={styles.table}>
                            <thead className={styles.thead}>
                                <tr>
                                    <th className={styles.th}>Origin</th>
                                    <th className={styles.th}>Added</th>
                                    <th className={styles.th} />
                                </tr>
                            </thead>
                            <tbody>
                                {origins.map(o => (
                                    <OriginRow key={o.id} origin={o} styles={styles} onRemoved={refetch} />
                                ))}
                            </tbody>
                        </table>
                    )}

                    <p className={styles.hint}>
                        Governs browser access only (CORS) — webhooks and other server-to-server callers
                        authenticate via API tokens instead and aren't affected by this list.
                    </p>

                    <div className={styles.addSection}>
                        <input
                            value={newOrigin}
                            disabled={addBusy}
                            onChange={e => setNewOrigin(e.target.value)}
                            onKeyDown={e => { if (e.key === 'Enter') void handleAdd(); }}
                            placeholder="https://example.com"
                            className={styles.addInput}
                        />
                        <button
                            type="button"
                            disabled={addBusy || !newOrigin.trim()}
                            onClick={() => void handleAdd()}
                            className={styles.addButton}
                        >
                            {addBusy ? 'Adding…' : 'Add origin'}
                        </button>
                    </div>
                    {addError && <p className={styles.addError}>{addError}</p>}
                </div>
            )}
        </div>
    );
}
