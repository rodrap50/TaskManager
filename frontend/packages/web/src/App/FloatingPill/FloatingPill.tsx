import { useState, type MouseEvent } from 'react';
import { ArrowLeft, LogOut, Moon, Shield, Sun } from 'lucide-react';
import { useFloatingPillStyles } from './FloatingPill.styles';

interface FloatingPillProps {
    showBack: boolean;
    onBack: () => void;
    theme: 'dark' | 'light';
    onToggleTheme: () => void;
    onLogout: () => void;
    isAdmin?: boolean;
    onOpenAdmin?: () => void;
}

const prefersReducedMotion = () =>
    typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

export function FloatingPill({ showBack, onBack, theme, onToggleTheme, onLogout, isAdmin, onOpenAdmin }: FloatingPillProps) {
    const styles = useFloatingPillStyles();
    const [spot, setSpot] = useState({ x: 50, y: 50 });
    const [hovering, setHovering] = useState(false);

    const handleMouseMove = (e: MouseEvent<HTMLDivElement>) => {
        if (prefersReducedMotion()) return;
        const rect = e.currentTarget.getBoundingClientRect();
        setSpot({
            x: ((e.clientX - rect.left) / rect.width) * 100,
            y: ((e.clientY - rect.top) / rect.height) * 100,
        });
    };

    const spotlightActive = hovering && !prefersReducedMotion();

    return (
        <div
            className={styles.container}
            style={{ animationPlayState: hovering ? 'paused' : 'running' }}
            onMouseMove={handleMouseMove}
            onMouseEnter={() => setHovering(true)}
            onMouseLeave={() => setHovering(false)}
        >
            <div
                className={styles.spotlight}
                style={{
                    opacity: spotlightActive ? 1 : 0,
                    background: `radial-gradient(60px circle at ${spot.x}% ${spot.y}%, color-mix(in srgb, var(--color-primary-700) 40%, transparent), transparent 70%)`,
                }}
            />
            <span className={styles.brandMark} role="img" aria-label="TaskManager" title="TaskManager" />
            {showBack && (
                <>
                    <span className={styles.divider} />
                    <button type="button" onClick={onBack} className={styles.iconButton} aria-label="Back to projects">
                        <ArrowLeft className={styles.iconSvg} />
                    </button>
                </>
            )}
            {isAdmin && (
                <>
                    <span className={styles.divider} />
                    <button
                        type="button"
                        onClick={onOpenAdmin}
                        className={styles.iconButton}
                        title="Admin panel"
                        aria-label="Open admin panel"
                    >
                        <Shield className={styles.iconSvg} />
                    </button>
                </>
            )}
            <span className={styles.divider} />
            <button
                type="button"
                onClick={onToggleTheme}
                className={styles.iconButton}
                title="DS01.3 temporary theme toggle — replace with the real Settings UI later"
                aria-label="Toggle theme"
            >
                {theme === 'dark' ? <Moon className={styles.iconSvg} /> : <Sun className={styles.iconSvg} />}
            </button>
            <span className={styles.divider} />
            <button
                type="button"
                onClick={onLogout}
                className={styles.iconButton}
                title="Log out"
                aria-label="Log out"
            >
                <LogOut className={styles.iconSvg} />
            </button>
        </div>
    );
}
