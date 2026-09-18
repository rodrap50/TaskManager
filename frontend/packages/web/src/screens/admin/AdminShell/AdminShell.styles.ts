export const useAdminShellStyles = () => ({
    // A 224px side rail leaves nothing beside it on a phone, so below `sm` the
    // nav becomes a horizontally scrollable strip across the top and the
    // content takes the full width underneath.
    root: 'flex h-full w-full flex-col bg-surface sm:flex-row',
    sideNav: 'flex w-full shrink-0 gap-1 overflow-x-auto border-b border-border bg-surface-raised px-3 py-2 ' +
        'sm:w-56 sm:flex-col sm:overflow-x-visible sm:border-b-0 sm:border-r sm:py-6',
    sideNavHeading: 'hidden px-3 pb-4 text-xs font-semibold uppercase tracking-wide text-text-muted sm:block',
    navLinkBase: 'flex min-h-11 shrink-0 items-center gap-2.5 whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium ' +
        'transition-colors sm:min-h-0',
    navLinkActive: 'bg-primary-800 dark:bg-primary-700 text-text-inverted',
    navLinkInactive: 'text-text-muted hover:bg-surface-overlay hover:text-text',
    navLinkDisabled: 'flex min-h-11 shrink-0 cursor-not-allowed items-center gap-2.5 whitespace-nowrap rounded-lg px-3 py-2 ' +
        'text-sm font-medium text-text-muted/50 sm:min-h-0',
    navIcon: 'h-4 w-4',
    content: 'min-h-0 flex-1 overflow-y-auto',
});
