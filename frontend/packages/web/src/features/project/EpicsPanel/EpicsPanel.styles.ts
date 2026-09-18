export const useEpicsPanelStyles = () => ({
    // z-[55]/z-[60] to clear the fixed FloatingNav — see TaskDetailPanel.styles.
    backdrop: 'fixed inset-0 z-[55] bg-black/50',
    panel: 'fixed inset-y-0 right-0 z-[60] flex w-[420px] max-w-full flex-col border-l border-border bg-surface-raised shadow-2xl ' +
        'animate-[drawerIn_220ms_ease-out] motion-reduce:animate-none',
    header: 'flex items-center justify-between gap-3 border-b border-border px-4 py-4 sm:px-6',
    titleText: 'text-base font-semibold text-text',
    closeButton: 'shrink-0 rounded-md p-2 text-text-muted transition-colors hover:bg-surface hover:text-text sm:p-1',
    closeIcon: 'h-4 w-4',
    body: 'flex-1 overflow-y-auto px-4 pt-4 pb-[calc(1rem+env(safe-area-inset-bottom))] sm:px-6',

    loadingText: 'text-sm text-text-muted',
    emptyText: 'text-sm text-text-muted',

    epicList: 'flex flex-col gap-1',
    epicRowWrap: 'py-1',
    epicRow: 'flex items-center justify-between gap-3 rounded-lg px-1 py-1.5',
    epicIdentity: 'flex min-w-0 items-center gap-2',
    colorDot: 'h-2.5 w-2.5 shrink-0 rounded-full border border-border-subtle',
    epicName: 'truncate text-sm font-medium text-text',
    rowActions: 'flex shrink-0 items-center gap-1',
    iconButton: 'shrink-0 rounded-md p-2.5 text-text-muted transition-colors hover:bg-surface hover:text-text ' +
        'disabled:cursor-not-allowed disabled:opacity-50 sm:p-1.5',
    deleteButton: 'shrink-0 rounded-md p-2.5 text-text-muted transition-colors hover:bg-surface hover:text-primary-700 ' +
        'disabled:cursor-not-allowed disabled:opacity-50 sm:p-1.5',
    icon: 'h-4 w-4',
    rowError: 'px-1 pb-2 text-xs text-primary-700',

    renameInput: 'w-full rounded-lg border border-border bg-surface px-2.5 py-1.5 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',

    addSection: 'mt-4 border-t border-border pt-4',
    addToggleButton: 'flex items-center gap-1.5 rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:text-text',
    addToggleIcon: 'h-3.5 w-3.5',
    createInput: 'mb-2 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    createButton: 'rounded-lg bg-primary-800 px-3 py-1.5 text-xs font-medium text-white transition-colors ' +
        'hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
    createRow: 'flex items-center gap-2',
    createError: 'mt-2 text-xs text-primary-700',
});
