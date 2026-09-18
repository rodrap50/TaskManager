export const useBoardScreenStyles = () => ({
    root: 'flex h-full w-full flex-col items-center',
    header: 'flex w-full max-w-[95vw] items-center justify-between gap-2 px-3 pt-4 pb-2 sm:gap-3 sm:px-4',
    titleWrap: 'inline-block min-w-0 rounded-br-xl border-b-2 border-r-2 px-3 py-1',
    projectName: 'truncate text-base font-semibold text-text sm:text-lg',
    headerActions: 'flex shrink-0 items-center gap-2 sm:gap-3',
    // Four controls plus the project name do not fit across a phone, so the
    // action buttons collapse to their icons below `sm` (each keeps an
    // aria-label in BoardScreen.tsx, since the visible text is what disappears).
    newTaskButton: 'flex min-h-9 items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-2.5 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800 sm:px-3',
    newTaskButtonIcon: 'h-3.5 w-3.5',
    membersButton: 'flex min-h-9 items-center gap-1.5 rounded-lg border border-border px-2.5 py-1.5 text-sm font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:text-text sm:px-3',
    membersButtonIcon: 'h-3.5 w-3.5',
    buttonLabel: 'hidden sm:inline',
    loadingText: 'p-8 text-center text-sm text-text-muted',
    errorText: 'p-8 text-center text-sm text-primary-700',
    emptyState: 'flex flex-1 flex-col items-center justify-center gap-3 px-4 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
