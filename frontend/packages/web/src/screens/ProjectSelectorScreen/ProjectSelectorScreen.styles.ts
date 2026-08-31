export const useProjectSelectorScreenStyles = () => ({
    container: 'flex flex-col items-center gap-7',
    header: 'flex w-full max-w-[80dvw] items-center justify-between pb-4',
    title: 'text-lg font-semibold text-text',
    newButton: 'flex items-center gap-1.5 rounded-lg bg-primary-800 dark:bg-primary-700 px-3 py-1.5 text-sm font-medium ' +
        'text-text-inverted transition-colors hover:bg-primary-700 dark:hover:bg-primary-800',
    newButtonIcon: 'h-3.5 w-3.5',
    loadingText: 'py-8 text-center text-sm text-text-muted',
    errorText: 'py-8 text-center text-sm text-primary-700',
    emptyState: 'flex w-full max-w-2xl flex-col items-center gap-3 rounded-xl border border-dashed border-border bg-surface-raised/50 py-16 text-center',
    emptyIcon: 'h-8 w-8 text-text-muted',
    emptyTitle: 'font-medium text-text',
    emptyHint: 'text-sm text-text-muted',
});
