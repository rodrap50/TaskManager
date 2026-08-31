import { useState, type FormEvent } from 'react';
import { setupAdmin, isConflictError } from '@taskmanager/shared';
import { useAuth } from '../../App/AuthContext';
import { useSetupScreenStyles } from './SetupScreen.styles';

export function SetupScreen({ onSetupUnavailable }: { onSetupUnavailable: () => void }) {
    const styles = useSetupScreenStyles();
    const { signIn } = useAuth();

    const [username, setUsername]               = useState('');
    const [password, setPassword]                = useState('');
    const [confirmPassword, setConfirmPassword]  = useState('');
    const [submitting, setSubmitting]            = useState(false);
    const [error, setError]                      = useState<string | null>(null);

    const passwordsMismatch = confirmPassword.length > 0 && password !== confirmPassword;
    const canSubmit = username.trim().length > 0 && password.length >= 8 && password === confirmPassword;

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!canSubmit) return;

        setSubmitting(true);
        setError(null);
        try {
            const result = await setupAdmin({ username: username.trim(), password });
            signIn(result.token);
        } catch (err) {
            if (isConflictError(err)) {
                // Someone else completed setup in the gap between our status check and this
                // submit — don't retry setup, fall back to the normal login screen instead.
                onSetupUnavailable();
                return;
            }
            setError('Could not complete setup. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={styles.container}>
            <form className={styles.panel} onSubmit={e => void handleSubmit(e)}>
                <p className={styles.brandMark}>TASKMANAGER</p>
                <h1 className={styles.heading}>Create the admin account</h1>
                <p className={styles.subheading}>One-time setup for a new TaskManager instance.</p>

                <label className={styles.label}>Username</label>
                <input
                    autoFocus
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    placeholder="Username"
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

                <label className={styles.label}>Confirm password</label>
                <input
                    type="password"
                    value={confirmPassword}
                    onChange={e => setConfirmPassword(e.target.value)}
                    placeholder="Re-enter password"
                    className={styles.input}
                />
                {passwordsMismatch && <p className={styles.hint}>Passwords don't match.</p>}

                {error && <div className={styles.errorBox}>{error}</div>}

                <button
                    type="submit"
                    disabled={!canSubmit || submitting}
                    className={styles.submitButton}
                >
                    {submitting ? 'Creating…' : 'Create admin account'}
                </button>
            </form>
        </div>
    );
}
