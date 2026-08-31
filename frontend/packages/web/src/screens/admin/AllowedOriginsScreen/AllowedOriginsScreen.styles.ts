export const useAllowedOriginsScreenStyles = () => ({
    container: 'flex h-full flex-col gap-4 p-6',
    header: 'flex items-center justify-between',
    title: 'text-lg font-semibold text-text',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',
    emptyText: 'p-8 text-center text-sm text-text-muted',

    tableWrap: 'flex-1 overflow-y-auto rounded-lg border border-border',
    table: 'w-full border-collapse text-left text-sm',
    thead: 'sticky top-0 bg-surface-raised',
    th: 'border-b border-border px-4 py-2.5 text-xs font-semibold uppercase tracking-wide text-text-muted',
    row: 'border-b border-border-subtle last:border-b-0 transition-colors hover:bg-surface-overlay',
    td: 'px-4 py-2.5 align-middle text-text',
    tdMuted: 'px-4 py-2.5 align-middle text-text-muted',

    originUrl: 'font-mono text-sm text-text',
    removeButton: 'inline-flex items-center justify-center rounded-md p-1.5 text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
    removeButtonIcon: 'h-4 w-4',

    rowError: 'px-4 pb-2 text-xs text-primary-700',

    addSection: 'flex items-center gap-2 border-t border-border px-4 py-3',
    addInput: 'flex-1 rounded-md border border-border bg-surface px-3 py-1.5 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none disabled:cursor-not-allowed disabled:opacity-50',
    addButton: 'rounded-md border border-border px-3 py-1.5 text-sm font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:bg-surface hover:text-text ' +
        'disabled:cursor-not-allowed disabled:opacity-50',
    addError: 'px-4 pb-3 text-xs text-primary-700',
    hint: 'px-4 pt-3 text-xs text-text-muted',
});
