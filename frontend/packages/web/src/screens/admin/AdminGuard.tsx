import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useAdminGuardStyles } from './AdminGuard.styles';

const MIN_DESKTOP_WIDTH = 1024;

interface AdminGuardProps {
    children: ReactNode;
}

export function AdminGuard({ children }: AdminGuardProps) {
    const styles = useAdminGuardStyles();
    const [isDesktop, setIsDesktop] = useState(() => window.innerWidth >= MIN_DESKTOP_WIDTH);

    useEffect(() => {
        const mql = window.matchMedia(`(min-width: ${MIN_DESKTOP_WIDTH}px)`);
        const onChange = () => setIsDesktop(mql.matches);
        onChange();
        mql.addEventListener('change', onChange);
        return () => mql.removeEventListener('change', onChange);
    }, []);

    if (!isDesktop) {
        return (
            <div className={styles.lockedRoot}>
                <h1 className={styles.heading}>Admin requires a desktop screen</h1>
                <p className={styles.body}>
                    This panel is only available at viewport widths of {MIN_DESKTOP_WIDTH}px or wider.
                    Resize your window or switch to a desktop device to continue.
                </p>
            </div>
        );
    }

    return <>{children}</>;
}
