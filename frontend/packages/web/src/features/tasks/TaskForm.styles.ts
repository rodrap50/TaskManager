export const useTaskFormStyles = () => ({
    label: 'mb-1 block text-sm font-medium text-text-muted',
    input: 'mb-4 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    priorityRow: 'mb-4 flex gap-2',
    priorityPillBase: 'flex-1 rounded-lg border py-1.5 text-sm font-medium transition-colors',
    priorityPillActive: 'border-transparent bg-primary-800 text-text-inverted',
    priorityPillInactive: 'border-border text-text-muted hover:border-border-subtle hover:text-text',
    dueDateWrap: 'mb-4',
    dueDateRow: 'flex items-center gap-2',
    dateInput: 'w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    clearDateButton: 'shrink-0 text-xs font-medium text-text-muted hover:text-text',
    noDueDateHint: 'mt-1 text-xs text-text-muted',
    assigneeRow: 'mb-4 flex gap-2 overflow-x-auto px-1 py-1',
    assigneeButton: 'shrink-0 rounded-full p-0.5 transition-shadow',
    assigneeSelected: 'ring-2 ring-primary-800',
    unassignedAvatar: 'flex h-9 w-9 items-center justify-center rounded-full border border-border ' +
        'bg-surface-overlay text-sm font-medium text-text-muted',
    errorBox: 'mb-4 rounded-md border border-primary-800/40 bg-primary-900/10 px-3 py-2 text-sm text-primary-700',
    actionsRow: 'flex gap-3 pt-2',
    cancelButton: 'flex-1 rounded-lg border border-border py-2 text-sm font-medium text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-text',
    submitButton: 'flex-1 rounded-lg bg-primary-800 py-2 text-sm font-medium text-text-inverted ' +
        'transition-colors hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
});
