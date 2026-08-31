export const useBoardScreenStyles = () => ({
    root: 'flex h-full flex-col items-center',
    header: 'flex items-center justify-between px-4 pt-4 pb-2 w-[95vw] ',
    titleWrap: 'inline-block rounded-br-xl border-b-2 border-r-2 px-3 py-1',
    projectName: 'truncate text-lg font-semibold text-text',
    headerActions: 'flex items-center gap-3',
    newTaskButton: 'flex items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-3 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
    newTaskButtonIcon: 'h-3.5 w-3.5',
    membersButton: 'flex items-center gap-1.5 rounded-lg border border-border px-3 py-1.5 text-sm font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:text-text',
    membersButtonIcon: 'h-3.5 w-3.5',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',
    emptyState: 'flex flex-1 flex-col items-center justify-center gap-3 px-4 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
