export const useProjectSelectorScreenStyles = () => ({
    container: 'flex w-full flex-col items-center gap-4 sm:gap-7',
    header: 'flex w-full max-w-[92dvw] items-center justify-between pb-2 sm:max-w-[80dvw] sm:pb-4',
    title: 'text-lg font-semibold text-text',
    newButton: 'flex min-h-9 items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-3 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
    newButtonIcon: 'h-3.5 w-3.5',
    loadingText: 'py-8 text-center text-sm text-text-muted',
    errorText: 'py-8 text-center text-sm text-primary-700',
    emptyState: 'flex w-[92dvw] max-w-2xl flex-col items-center gap-3 rounded-xl border border-dashed border-border bg-surface-raised/50 px-4 py-16 text-center sm:w-full',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
