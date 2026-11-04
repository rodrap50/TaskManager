export const useArchivedPanelStyles = () => ({
    // Same drawer shell as EpicsPanel/MembersPanel; z-[55]/z-[60] clear the fixed FloatingNav.
    backdrop: 'fixed inset-0 z-[55] bg-black/50',
    panel: 'fixed inset-y-0 right-0 z-[60] flex w-[420px] max-w-full flex-col border-l border-border bg-surface-raised shadow-2xl ' +
        'animate-[drawerIn_220ms_ease-out] motion-reduce:animate-none',
    header: 'flex items-center justify-between gap-3 border-b border-border px-4 py-4 sm:px-6',
    titleText: 'text-base font-semibold text-text',
    closeButton: 'shrink-0 rounded-md p-2 text-text-muted transition-colors hover:bg-surface hover:text-text sm:p-1',
    closeIcon: 'h-4 w-4',
    body: 'flex-1 overflow-y-auto px-4 pt-4 pb-[calc(1rem+env(safe-area-inset-bottom))] sm:px-6',

    archiveSection: 'mb-4 border-b border-border pb-4',
    archiveButton: 'rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium text-text-muted transition-colors ' +
        'hover:border-border-subtle hover:text-text disabled:cursor-not-allowed disabled:opacity-50',
    hint: 'mt-2 text-xs text-text-muted',

    searchInput: 'mb-3 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',

    mutedText: 'text-sm text-text-muted',
    errorText: 'mb-2 text-xs text-primary-700',

    list: 'flex flex-col gap-1',
    row: 'flex items-center justify-between gap-3 rounded-lg px-1 py-1.5',
    rowText: 'min-w-0',
    rowTitle: 'truncate text-sm font-medium text-text',
    rowMeta: 'text-xs text-text-muted',
    restoreButton: 'shrink-0 rounded-md p-2.5 text-text-muted transition-colors hover:bg-surface hover:text-text ' +
        'disabled:cursor-not-allowed disabled:opacity-50 sm:p-1.5',
    icon: 'h-4 w-4',
});
