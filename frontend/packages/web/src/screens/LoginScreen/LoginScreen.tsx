import { useState, type FormEvent } from 'react';
import { login } from '@taskmanager/shared';
import { useAuth } from '../../App/AuthContext';
import { useLoginScreenStyles } from './LoginScreen.styles';

export function LoginScreen() {
    const styles = useLoginScreenStyles();
    const { signIn } = useAuth();

    const [username, setUsername]     = useState('');
    const [password, setPassword]     = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [error, setError]           = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!username.trim() || !password) return;

        setSubmitting(true);
        setError(null);
        try {
            const result = await login({ username: username.trim(), password });
            signIn(result.token);
        } catch {
            // Deliberately doesn't distinguish "unknown user" from "wrong password",
            // matching the backend's own 401 (AUTH01.1).
            setError('Invalid username or password.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={styles.container}>
            <form className={styles.panel} onSubmit={e => void handleSubmit(e)}>
                <p className={styles.brandMark}>TASKMANAGER</p>
                <h1 className={styles.heading}>Sign in</h1>
                <p className={styles.subheading}>Use your TaskManager account to continue.</p>

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
                    placeholder="Password"
                    className={styles.input}
                />

                {error && <div className={styles.errorBox}>{error}</div>}

                <button
                    type="submit"
                    disabled={!username.trim() || !password || submitting}
                    className={styles.submitButton}
                >
                    {submitting ? 'Signing in…' : 'Sign in'}
                </button>
            </form>
        </div>
    );
}
