import { useState, type FormEvent } from 'react';
import { createApiToken, getErrorMessage, type CreateApiTokenResult } from '@taskmanager/shared';
import { Modal } from '../../../../components/Modal';
import { useCreateApiTokenModalStyles } from './CreateApiTokenModal.styles';

interface CreateApiTokenModalProps {
    onClose: () => void;
    onCreated: (result: CreateApiTokenResult) => void | Promise<void>;
}

export function CreateApiTokenModal({ onClose, onCreated }: CreateApiTokenModalProps) {
    const styles = useCreateApiTokenModalStyles();

    const [name, setName]                             = useState('');
    const [isReadOnly, setIsReadOnly]                  = useState(false);
    const [expiresAt, setExpiresAt]                    = useState('');
    const [rateLimitPerMinute, setRateLimitPerMinute]  = useState('');
    const [submitting, setSubmitting]                  = useState(false);
    const [error, setError]                            = useState<string | null>(null);

    const canSubmit = name.trim() && !submitting;

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!canSubmit) return;

        setSubmitting(true);
        setError(null);
        try {
            const result = await createApiToken({
                name: name.trim(),
                isReadOnly,
                expiresAt: expiresAt || undefined,
                rateLimitPerMinute: rateLimitPerMinute ? Number(rateLimitPerMinute) : undefined,
            });
            await onCreated(result);
        } catch (err) {
            setError(getErrorMessage(err, 'Failed to create API token'));
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Modal isOpen onClose={onClose} title="New API Token">
            <form onSubmit={e => void handleSubmit(e)}>
                <label className={styles.label}>Name</label>
                <input
                    autoFocus
                    value={name}
                    onChange={e => setName(e.target.value)}
                    placeholder="Home Assistant"
                    className={styles.input}
                />

                <label className={styles.label}>Expires (optional)</label>
                <input
                    type="date"
                    value={expiresAt}
                    onChange={e => setExpiresAt(e.target.value)}
                    className={styles.input}
                />

                <label className={styles.label}>Rate limit per minute (optional)</label>
                <input
                    type="number"
                    min={1}
                    value={rateLimitPerMinute}
                    onChange={e => setRateLimitPerMinute(e.target.value)}
                    placeholder="No limit"
                    className={styles.input}
                />

                <div className={styles.toggleRow}>
                    <div>
                        <div className={styles.toggleLabel}>Read-only</div>
                        <div className={styles.toggleHint}>Blocks create/update/delete calls with this token</div>
                    </div>
                    <button
                        type="button"
                        onClick={() => setIsReadOnly(v => !v)}
                        aria-pressed={isReadOnly}
                        aria-label="Toggle read-only"
                        className={`${styles.toggleButtonBase} ${isReadOnly ? styles.toggleButtonOn : styles.toggleButtonOff}`}
                    >
                        <span className={`${styles.toggleKnob} ${isReadOnly ? styles.toggleKnobOn : styles.toggleKnobOff}`} />
                    </button>
                </div>

                {error && <div className={styles.errorBox}>{error}</div>}

                <div className={styles.actionsRow}>
                    <button type="button" onClick={onClose} className={styles.cancelButton}>
                        Cancel
                    </button>
                    <button type="submit" disabled={!canSubmit} className={styles.submitButton}>
                        {submitting ? 'Creating…' : 'Create'}
                    </button>
                </div>
            </form>
        </Modal>
    );
}
