import { useEffect, useState } from 'react';
import { getSetupStatus } from '@taskmanager/shared';
import { ProjectProvider, useProject } from '../contexts/ProjectContext';
import { ProjectSelectorScreen } from '../screens/ProjectSelectorScreen';
import { BoardScreen } from '../screens/BoardScreen';
import { LoginScreen } from '../screens/LoginScreen';
import { SetupScreen } from '../screens/SetupScreen';
import { useAppStyles } from './App.styles';
import { FloatingPill } from './FloatingPill';
import { FloatingNav, type Tab } from './FloatingNav';
import { AuthProvider, useAuth } from './AuthContext';
import { AdminShell } from '../screens/admin';

type Theme = 'dark' | 'light';
const THEME_KEY = 'tm:theme';

function loadTheme(): Theme {
    return localStorage.getItem(THEME_KEY) === 'light' ? 'light' : 'dark';
}

function AppShell() {
    const { selectedProjectId, selectProject } = useProject();
    const { user, logout } = useAuth();
    const [theme, setTheme] = useState<Theme>(loadTheme);
    const [viewingAdmin, setViewingAdmin] = useState(false);
    const styles = useAppStyles();
    const activeTab: Tab = selectedProjectId ? 'board' : 'projects';

    useEffect(() => {
        document.documentElement.classList.toggle('light', theme === 'light');
        localStorage.setItem(THEME_KEY, theme);
    }, [theme]);

    const handleTabChange = (tab: Tab) => {
        if (tab === 'projects') {
            setViewingAdmin(false);
            selectProject(null);
        }
    };

    return (
        <div className={styles.root}>
            <FloatingPill
                showBack={viewingAdmin || !!selectedProjectId}
                onBack={() => (viewingAdmin ? setViewingAdmin(false) : selectProject(null))}
                theme={theme}
                onToggleTheme={() => setTheme(t => (t === 'dark' ? 'light' : 'dark'))}
                onLogout={logout}
                isAdmin={user?.isAdmin}
                onOpenAdmin={() => setViewingAdmin(true)}
            />

            <main className={styles.main}>
                {viewingAdmin ? <AdminShell /> : (selectedProjectId ? <BoardScreen /> : <ProjectSelectorScreen />)}
            </main>

            <FloatingNav active={activeTab} onChange={handleTabChange} />
        </div>
    );
}

function AppContent() {
    const { user } = useAuth();
    const styles = useAppStyles();
    // null = still checking; re-fetched on every transition into the unauthenticated
    // state (not just the very first mount) so a fresh page load — or a logout — never
    // trusts a stale flag about whether setup is still required.
    const [setupRequired, setSetupRequired] = useState<boolean | null>(null);

    useEffect(() => {
        if (user) return;

        let cancelled = false;
        getSetupStatus()
            .then(status => { if (!cancelled) setSetupRequired(status.setupRequired); })
            .catch(() => { if (!cancelled) setSetupRequired(false); });

        return () => { cancelled = true; };
    }, [user]);

    // Single choke point: no protected screen mounts unless a valid session exists.
    if (!user) {
        if (setupRequired === null) return <div className={styles.loading}>Loading…</div>;
        return setupRequired
            ? <SetupScreen onSetupUnavailable={() => setSetupRequired(false)} />
            : <LoginScreen />;
    }

    return (
        <ProjectProvider>
            <AppShell />
        </ProjectProvider>
    );
}

function App() {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    );
}

export default App;
