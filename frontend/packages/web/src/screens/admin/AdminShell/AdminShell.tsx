import { useState } from 'react';
import { Users, Webhook, Gauge, Globe } from 'lucide-react';
import { AdminGuard } from '../AdminGuard';
import { UserManagementScreen } from '../UserManagementScreen';
import { ProjectCriticalityScreen } from '../ProjectCriticalityScreen';
import { AllowedOriginsScreen } from '../AllowedOriginsScreen';
import { useAdminShellStyles } from './AdminShell.styles';

type AdminScreen = 'users' | 'projectCriticality' | 'allowedOrigins';

export function AdminShell() {
    const styles = useAdminShellStyles();
    const [activeScreen, setActiveScreen] = useState<AdminScreen>('users');

    return (
        <AdminGuard>
            <div className={styles.root}>
                <nav className={styles.sideNav}>
                    <span className={styles.sideNavHeading}>Admin</span>
                    <button
                        type="button"
                        onClick={() => setActiveScreen('users')}
                        className={`${styles.navLinkBase} ${activeScreen === 'users' ? styles.navLinkActive : styles.navLinkInactive}`}
                    >
                        <Users className={styles.navIcon} />
                        User Management
                    </button>
                    <button
                        type="button"
                        onClick={() => setActiveScreen('projectCriticality')}
                        className={`${styles.navLinkBase} ${activeScreen === 'projectCriticality' ? styles.navLinkActive : styles.navLinkInactive}`}
                    >
                        <Gauge className={styles.navIcon} />
                        Project Criticality
                    </button>
                    <button
                        type="button"
                        onClick={() => setActiveScreen('allowedOrigins')}
                        className={`${styles.navLinkBase} ${activeScreen === 'allowedOrigins' ? styles.navLinkActive : styles.navLinkInactive}`}
                    >
                        <Globe className={styles.navIcon} />
                        Allowed Origins
                    </button>
                    <span
                        className={styles.navLinkDisabled}
                        title="Webhook Manager is not yet available"
                        aria-disabled="true"
                    >
                        <Webhook className={styles.navIcon} />
                        Webhook Manager
                    </span>
                </nav>

                <div className={styles.content}>
                    {activeScreen === 'users' && <UserManagementScreen />}
                    {activeScreen === 'projectCriticality' && <ProjectCriticalityScreen />}
                    {activeScreen === 'allowedOrigins' && <AllowedOriginsScreen />}
                </div>
            </div>
        </AdminGuard>
    );
}
