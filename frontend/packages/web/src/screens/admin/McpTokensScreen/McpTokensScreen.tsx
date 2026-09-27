import { useState } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import { revokeApiToken, getErrorMessage, type ApiTokenDto, type CreateApiTokenResult } from '@taskmanager/shared';
import { useApiTokens } from '../../../hooks/useApiTokens';
import { Modal } from '../../../components/Modal';
import { CreateApiTokenModal } from './CreateApiTokenModal';
import { useMcpTokensScreenStyles } from './McpTokensScreen.styles';

type Styles = ReturnType<typeof useMcpTokensScreenStyles>;

function formatDate(value: string | null) {
    return value ? new Date(value).toLocaleDateString() : '—';
}

interface TokenRowProps {
    token: ApiTokenDto;
    styles: Styles;
    onRevoked: () => void | Promise<void>;
}

function TokenRow({ token, styles, onRevoked }: TokenRowProps) {
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const isRevoked = token.revokedAt !== null;

    const handleRevoke = async () => {
        setBusy(true);
        setError(null);
        try {
            await revokeApiToken(token.id);
            await onRevoked();
        } catch (e) {
            setError(getErrorMessage(e, 'Failed to revoke token'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <>
            <tr className={`${styles.row} ${isRevoked ? styles.rowRevoked : ''}`}>
                <td className={styles.td}>
                    <span className={styles.tokenName}>{token.name}</span>
                </td>
                <td className={styles.td}>
                    {token.isReadOnly
                        ? <span className={styles.readOnlyBadge}>Read-only</span>
                        : <span className={styles.tdMuted}>—</span>}
                </td>
                <td className={styles.tdMuted}>{formatDate(token.expiresAt)}</td>
                <td className={styles.tdMuted}>{token.rateLimitPerMinute ?? '—'}</td>
                <td className={styles.tdMuted}>{formatDate(token.createdAt)}</td>
                <td className={styles.td}>
                    <span className={`${styles.statusBadge} ${isRevoked ? styles.statusRevoked : styles.statusActive}`}>
                        <span className={styles.statusDot} />
                        {isRevoked ? 'Revoked' : 'Active'}
                    </span>
                </td>
                <td className={styles.td}>
                    {!isRevoked && (
                        <button
                            type="button"
                            disabled={busy}
                            onClick={() => void handleRevoke()}
                            className={styles.revokeButton}
                            aria-label={`Revoke ${token.name}`}
                            title="Revoke token"
                        >
                            <Trash2 className={styles.revokeButtonIcon} />
                        </button>
                    )}
                </td>
            </tr>
            {error && (
                <tr>
                    <td colSpan={7} className={styles.rowError}>{error}</td>
                </tr>
            )}
        </>
    );
}

interface RevealTokenModalProps {
    token: string;
    styles: Styles;
    onDone: () => void;
}

function RevealTokenModal({ token, styles, onDone }: RevealTokenModalProps) {
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        try {
            await navigator.clipboard.writeText(token);
            setCopied(true);
        } catch {
            // Clipboard access denied/unavailable — the token is still visible and
            // selectable by hand in the box below, so this isn't a dead end.
        }
    };

    return (
        <Modal isOpen onClose={onDone} title="API Token Created">
            <div className={styles.warningBox}>
                Copy this token now — you won't be able to see it again after closing this dialog.
            </div>
            <div className={styles.tokenRow}>
                <code className={styles.tokenValue}>{token}</code>
                <button type="button" onClick={() => void handleCopy()} className={styles.copyButton}>
                    {copied ? 'Copied!' : 'Copy'}
                </button>
            </div>
            <button type="button" onClick={onDone} className={styles.doneButton}>
                Done
            </button>
        </Modal>
    );
}

/**
 * MCP06 — admin screen for managing ApiTokens (MCP02's read-only/expiry/rate-limit scoped
 * credentials), used by webhooks, MCP clients, and other service-to-service callers.
 */
export function McpTokensScreen() {
    const { data: tokens, isLoading, error, refetch } = useApiTokens();
    const styles = useMcpTokensScreenStyles();

    const [showCreate, setShowCreate] = useState(false);
    const [revealedToken, setRevealedToken] = useState<string | null>(null);

    const handleCreated = async (result: CreateApiTokenResult) => {
        setShowCreate(false);
        setRevealedToken(result.token);
        await refetch();
    };

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>MCP Tokens</h2>
                <button type="button" onClick={() => setShowCreate(true)} className={styles.newButton}>
                    <Plus className={styles.newButtonIcon} />
                    New Token
                </button>
            </div>

            {isLoading && <p className={styles.loadingText}>Loading…</p>}
            {error && <p className={styles.errorText}>Failed to load API tokens — check the API is running</p>}

            {!isLoading && !error && (
                <div className={styles.tableWrap}>
                    {tokens.length === 0 ? (
                        <p className={styles.emptyText}>No API tokens yet.</p>
                    ) : (
                        <table className={styles.table}>
                            <thead className={styles.thead}>
                                <tr>
                                    <th className={styles.th}>Name</th>
                                    <th className={styles.th}>Access</th>
                                    <th className={styles.th}>Expires</th>
                                    <th className={styles.th}>Rate Limit</th>
                                    <th className={styles.th}>Created</th>
                                    <th className={styles.th}>Status</th>
                                    <th className={styles.th} />
                                </tr>
                            </thead>
                            <tbody>
                                {tokens.map(t => (
                                    <TokenRow key={t.id} token={t} styles={styles} onRevoked={refetch} />
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
            )}

            {showCreate && (
                <CreateApiTokenModal onClose={() => setShowCreate(false)} onCreated={handleCreated} />
            )}

            {revealedToken && (
                <RevealTokenModal token={revealedToken} styles={styles} onDone={() => setRevealedToken(null)} />
            )}
        </div>
    );
}
