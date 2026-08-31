export const useTodoListStyles = () => ({
    container: 'h-full overflow-y-auto w-[60vw]',
    rowWrap: 'w-full cursor-pointer border-b border-border bg-surface-raised px-4 py-3 text-left ' +
        'transition-colors duration-200 ease-out hover:bg-surface-overlay',
    row: 'flex w-full items-center gap-3',
    rowError: 'mt-1.5 pl-8 text-xs text-primary-700',
    checkbox: 'flex h-5 w-5 shrink-0 items-center justify-center rounded-md border-2 border-border-subtle ' +
        'transition-colors duration-150 hover:border-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
    checkboxChecked: 'border-primary-700 bg-primary-700',
    checkIcon: 'h-3.5 w-3.5 text-white',
    title: 'flex-1 truncate text-sm font-medium text-text',
    titleDone: 'flex-1 truncate text-sm font-medium text-text-muted line-through',
    metaRow: 'flex flex-shrink-0 items-center gap-2',
    dueNormal: 'text-[11px] font-medium text-text-muted',
    dueOverdue: 'text-[11px] font-medium text-primary-700',
    emptyState: 'flex h-full flex-col items-center justify-center gap-3 px-4 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
