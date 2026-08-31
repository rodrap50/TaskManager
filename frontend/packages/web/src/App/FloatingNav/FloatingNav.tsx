import { Folder, Kanban, Settings as SettingsIcon } from 'lucide-react';
import { useFloatingNavStyles } from './FloatingNav.styles';

export type Tab = 'board' | 'projects' | 'settings';

interface FloatingNavProps {
    active: Tab;
    onChange: (tab: Tab) => void;
}

const TABS: { id: Tab; label: string; Icon: typeof Kanban }[] = [
    { id: 'board',    label: 'Board',    Icon: Kanban },
    { id: 'projects', label: 'Projects', Icon: Folder },
    { id: 'settings', label: 'Settings', Icon: SettingsIcon },
];

export function FloatingNav({ active, onChange }: FloatingNavProps) {
    const styles = useFloatingNavStyles();
    return (
        <nav className={styles.container}>
            {TABS.map(({ id, label, Icon }) => (
                <button
                    type="button"
                    key={id}
                    onClick={() => onChange(id)}
                    className={`${styles.tabBase} ${active === id ? styles.tabActive : styles.tabInactive}`}
                >
                    <Icon className={styles.icon} />
                    {label}
                </button>
            ))}
        </nav>
    );
}
