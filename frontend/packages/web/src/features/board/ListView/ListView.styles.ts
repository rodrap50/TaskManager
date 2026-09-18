export const useListViewStyles = () => ({
    container: 'min-h-0 w-full flex-1 overflow-y-auto sm:w-[90vw] lg:w-[60vw]',
    // Below `sm` the meta chips are forced onto their own line (basis-full, the
    // same trick the KanbanBoard toast uses) so the title keeps a full row to
    // itself instead of being squeezed to a few characters by priority/score/
    // epic/due/avatar all competing for a phone-width row.
    row: 'flex min-h-14 w-full flex-wrap items-center gap-x-3 gap-y-1.5 border-b border-border bg-surface-raised px-4 py-3 text-left ' +
        'transition-colors duration-200 ease-out hover:bg-surface-overlay hover:shadow-[0_4px_16px_-6px_rgba(185,28,28,0.3)] ' +
        'active:scale-[0.995]',
    title: 'min-w-0 flex-1 truncate text-sm font-medium text-text',
    metaRow: 'flex flex-shrink-0 flex-wrap items-center gap-2 max-sm:basis-full',
    phaseTag: 'rounded border border-border-subtle bg-surface-overlay px-1.5 py-0.5 text-[11px] text-text-muted',
    epicTag: 'truncate rounded px-1.5 py-0.5 text-[11px] font-medium text-white',
    dueNormal: 'text-[11px] font-medium text-text-muted',
    dueOverdue: 'text-[11px] font-medium text-primary-700',
    emptyState: 'flex h-full flex-col items-center justify-center gap-3 px-4 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
