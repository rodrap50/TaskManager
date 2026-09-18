export const useProjectCriticalityScreenStyles = () => ({
    container: 'flex h-full flex-col gap-4 p-3 sm:p-6',
    header: 'flex items-center justify-between',
    title: 'text-lg font-semibold text-text',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',

    tableWrap: 'flex-1 overflow-auto rounded-lg border border-border',
    table: 'w-full min-w-[480px] border-collapse text-left text-sm',
    thead: 'sticky top-0 bg-surface-raised',
    th: 'border-b border-border px-3 py-2.5 text-xs font-semibold uppercase tracking-wide text-text-muted sm:px-4',
    row: 'border-b border-border-subtle last:border-b-0 transition-colors hover:bg-surface-overlay',
    td: 'px-3 py-2.5 align-middle text-text sm:px-4',
    tdMuted: 'px-3 py-2.5 align-middle text-text-muted sm:px-4',

    projectName: 'font-medium text-text',
    scoreCell: 'flex items-center gap-2',
    scoreInput: 'w-16 rounded-md border border-border bg-surface px-2 py-1 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none disabled:cursor-not-allowed disabled:opacity-50',
    saveButton: 'min-h-9 shrink-0 rounded-md border border-border px-2.5 py-1 text-xs font-medium text-text-muted sm:min-h-0 ' +
        'transition-colors hover:border-border-subtle hover:bg-surface hover:text-text ' +
        'disabled:cursor-not-allowed disabled:opacity-50',

    rowError: 'px-4 pb-2 text-xs text-primary-700',
});
