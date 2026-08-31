export const useAdminShellStyles = () => ({
    root: 'flex h-full w-full bg-surface',
    sideNav: 'flex w-56 shrink-0 flex-col gap-1 border-r border-border bg-surface-raised px-3 py-6',
    sideNavHeading: 'px-3 pb-4 text-xs font-semibold uppercase tracking-wide text-text-muted',
    navLinkBase: 'flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
    navLinkActive: 'bg-primary-800 dark:bg-primary-700 text-text-inverted',
    navLinkInactive: 'text-text-muted hover:bg-surface-overlay hover:text-text',
    navLinkDisabled: 'flex cursor-not-allowed items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-text-muted/50',
    navIcon: 'h-4 w-4',
    content: 'flex-1 overflow-y-auto',
});
