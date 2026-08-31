export const useUserManagementScreenStyles = () => ({
    container: 'flex h-full flex-col gap-4 p-6',
    header: 'flex items-center justify-between',
    title: 'text-lg font-semibold text-text',
    newButton: 'flex items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-3 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
    newButtonIcon: 'h-3.5 w-3.5',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',

    tableWrap: 'flex-1 overflow-y-auto rounded-lg border border-border',
    table: 'w-full border-collapse text-left text-sm',
    thead: 'sticky top-0 bg-surface-raised',
    th: 'border-b border-border px-4 py-2.5 text-xs font-semibold uppercase tracking-wide text-text-muted',
    row: 'border-b border-border-subtle last:border-b-0 transition-colors hover:bg-surface-overlay',
    rowInactive: 'opacity-50',
    td: 'px-4 py-2.5 align-middle text-text',
    tdMuted: 'px-4 py-2.5 align-middle text-text-muted',

    identityCell: 'flex items-center gap-2.5',
    avatarWrap: 'relative shrink-0',
    username: 'font-medium text-text',

    statusBadge: 'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium',
    statusActive: 'bg-emerald-500/10 text-emerald-500',
    statusInactive: 'bg-text-muted/10 text-text-muted',
    statusDot: 'h-1.5 w-1.5 rounded-full bg-current',

    adminBadge: 'inline-flex items-center rounded-full border border-primary-700/40 bg-primary-800/10 ' +
        'px-2 py-0.5 text-xs font-medium text-primary-700',

    actionsCell: 'flex items-center gap-2',
    actionButton: 'rounded-md border border-border px-2.5 py-1 text-xs font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:bg-surface hover:text-text ' +
        'disabled:cursor-not-allowed disabled:opacity-50',
    avatarUploadButton: 'absolute -bottom-1 -right-1 flex h-5 w-5 items-center justify-center rounded-full ' +
        'border border-border bg-surface-raised text-text-muted transition-colors ' +
        'hover:border-border-subtle hover:text-text disabled:cursor-not-allowed disabled:opacity-50',
    hiddenFileInput: 'hidden',
    avatarUploadIcon: 'h-3 w-3',

    rowError: 'px-4 pb-2 text-xs text-primary-700',
});
