import { useState, type FormEvent } from 'react';
import { createUser, getErrorMessage } from '@taskmanager/shared';
import { Modal } from '../../../../components/Modal';
import { useCreateUserModalStyles } from './CreateUserModal.styles';

interface CreateUserModalProps {
    onClose: () => void;
    onCreated: () => void | Promise<void>;
}

export function CreateUserModal({ onClose, onCreated }: CreateUserModalProps) {
    const styles = useCreateUserModalStyles();

    const [username, setUsername]       = useState('');
    const [displayName, setDisplayName] = useState('');
    const [email, setEmail]             = useState('');
    const [password, setPassword]       = useState('');
    const [isAdmin, setIsAdmin]         = useState(false);
    const [submitting, setSubmitting]   = useState(false);
    const [error, setError]             = useState<string | null>(null);

    const canSubmit = username.trim() && displayName.trim() && email.trim() && password && !submitting;

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!canSubmit) return;

        setSubmitting(true);
        setError(null);
        try {
            await createUser({
                username: username.trim(),
                displayName: displayName.trim(),
                email: email.trim(),
                password,
                isAdmin,
            });
            await onCreated();
            onClose();
        } catch (err) {
            setError(getErrorMessage(err, 'Failed to create user'));
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Modal isOpen onClose={onClose} title="New User">
            <form onSubmit={e => void handleSubmit(e)}>
                <label className={styles.label}>Username</label>
                <input
                    autoFocus
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    placeholder="jdoe"
                    className={styles.input}
                />

                <label className={styles.label}>Display Name</label>
                <input
                    value={displayName}
                    onChange={e => setDisplayName(e.target.value)}
                    placeholder="Jane Doe"
                    className={styles.input}
                />

                <label className={styles.label}>Email</label>
                <input
                    type="email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    placeholder="jane@example.com"
                    className={styles.input}
                />

                <label className={styles.label}>Password</label>
                <input
                    type="password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    placeholder="At least 8 characters"
                    className={styles.input}
                />

                <div className={styles.toggleRow}>
                    <div>
                        <div className={styles.toggleLabel}>Admin</div>
                        <div className={styles.toggleHint}>Grants full admin panel access</div>
                    </div>
                    <button
                        type="button"
                        onClick={() => setIsAdmin(v => !v)}
                        aria-pressed={isAdmin}
                        aria-label="Toggle admin"
                        className={`${styles.toggleButtonBase} ${isAdmin ? styles.toggleButtonOn : styles.toggleButtonOff}`}
                    >
                        <span className={`${styles.toggleKnob} ${isAdmin ? styles.toggleKnobOn : styles.toggleKnobOff}`} />
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
