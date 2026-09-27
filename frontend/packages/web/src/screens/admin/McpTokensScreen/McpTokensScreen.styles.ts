export const useMcpTokensScreenStyles = () => ({
    container: 'flex h-full flex-col gap-4 p-6',
    header: 'flex items-center justify-between',
    title: 'text-lg font-semibold text-text',
    newButton: 'flex items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-3 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
    newButtonIcon: 'h-3.5 w-3.5',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',
    emptyText: 'p-8 text-center text-sm text-text-muted',

    tableWrap: 'flex-1 overflow-y-auto rounded-lg border border-border',
    table: 'w-full border-collapse text-left text-sm',
    thead: 'sticky top-0 bg-surface-raised',
    th: 'border-b border-border px-4 py-2.5 text-xs font-semibold uppercase tracking-wide text-text-muted',
    row: 'border-b border-border-subtle last:border-b-0 transition-colors hover:bg-surface-overlay',
    rowRevoked: 'opacity-50',
    td: 'px-4 py-2.5 align-middle text-text',
    tdMuted: 'px-4 py-2.5 align-middle text-text-muted',
    tokenName: 'font-medium text-text',

    badge: 'inline-flex items-center rounded-full border border-border px-2 py-0.5 text-xs font-medium text-text-muted',
    readOnlyBadge: 'inline-flex items-center rounded-full border border-primary-700/40 bg-primary-800/10 ' +
        'px-2 py-0.5 text-xs font-medium text-primary-700',

    statusBadge: 'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium',
    statusActive: 'bg-emerald-500/10 text-emerald-500',
    statusRevoked: 'bg-text-muted/10 text-text-muted',
    statusDot: 'h-1.5 w-1.5 rounded-full bg-current',

    revokeButton: 'inline-flex items-center justify-center rounded-md p-1.5 text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
    revokeButtonIcon: 'h-4 w-4',
    rowError: 'px-4 pb-2 text-xs text-primary-700',

    // Reveal-token dialog (shown once, immediately after creation)
    warningBox: 'mb-3 rounded-md border border-primary-800/40 bg-primary-900/10 px-3 py-2 text-sm text-primary-700',
    tokenRow: 'flex items-center gap-2',
    tokenValue: 'flex-1 overflow-x-auto rounded-md border border-border bg-surface px-3 py-2 ' +
        'font-mono text-sm text-text',
    copyButton: 'shrink-0 rounded-md border border-border px-3 py-2 text-sm font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:bg-surface hover:text-text',
    doneButton: 'mt-4 w-full rounded-lg bg-primary-800 dark:bg-primary-700 py-2 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
});
