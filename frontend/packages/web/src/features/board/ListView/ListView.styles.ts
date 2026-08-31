export const useListViewStyles = () => ({
    container: 'h-full overflow-y-auto w-[60vw]',
    row: 'flex w-full items-center gap-3 border-b border-border bg-surface-raised px-4 py-3 text-left ' +
        'transition-colors duration-200 ease-out hover:bg-surface-overlay hover:shadow-[0_4px_16px_-6px_rgba(185,28,28,0.3)] ' +
        'active:scale-[0.995]',
    title: 'flex-1 truncate text-sm font-medium text-text',
    metaRow: 'flex flex-shrink-0 items-center gap-2',
    phaseTag: 'rounded border border-border-subtle bg-surface-overlay px-1.5 py-0.5 text-[11px] text-text-muted',
    epicTag: 'truncate rounded px-1.5 py-0.5 text-[11px] font-medium text-white',
    dueNormal: 'text-[11px] font-medium text-text-muted',
    dueOverdue: 'text-[11px] font-medium text-primary-700',
    emptyState: 'flex h-full flex-col items-center justify-center gap-3 px-4 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
