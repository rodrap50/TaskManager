export const useTaskDetailPanelStyles = () => ({
    backdrop: 'fixed inset-0 z-40 bg-black/50',
    panel: 'fixed inset-y-0 right-0 z-50 flex w-[480px] max-w-full flex-col border-l border-border bg-surface-raised shadow-2xl ' +
        'animate-[drawerIn_220ms_ease-out] motion-reduce:animate-none',
    header: 'flex items-start justify-between gap-3 border-b border-border px-6 py-4',
    titleText: 'flex-1 truncate text-base font-semibold text-text',
    titleInput: 'flex-1 rounded-lg border border-primary-700 bg-surface px-2 py-1 text-base font-semibold text-text ' +
        'focus:outline-none',
    headerActions: 'flex shrink-0 items-center gap-2',
    editButton: 'flex items-center gap-1.5 rounded-lg border border-border px-2.5 py-1 text-xs font-medium text-text-muted ' +
        'transition-colors hover:border-border-subtle hover:text-text',
    editButtonIcon: 'h-3.5 w-3.5',
    closeButton: 'shrink-0 rounded-md p-1 text-text-muted transition-colors hover:bg-surface hover:text-text',
    closeIcon: 'h-4 w-4',
    body: 'flex-1 overflow-y-auto px-6 py-4',
    descriptionText: 'mb-5 text-sm text-text',
    descriptionPlaceholder: 'mb-5 text-sm italic text-text-muted',
    descriptionInput: 'mb-5 w-full resize-none rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    sectionLabel: 'mb-1.5 text-xs font-medium uppercase tracking-wide text-text-muted',
    readOnlyValue: 'mb-5 text-sm font-medium text-text',
    readOnlyAssignee: 'mb-5 flex items-center gap-2',
    pillRow: 'mb-5 flex flex-wrap gap-2',
    pillBase: 'rounded-full border px-3 py-1 text-xs font-medium transition-colors',
    pillActive: 'border-transparent bg-primary-800 text-text-inverted',
    pillInactive: 'border-border text-text-muted hover:border-border-subtle hover:text-text',
    assigneeRow: 'mb-5 flex gap-2 overflow-x-auto px-1 py-1',
    assigneeButton: 'shrink-0 rounded-full p-0.5 transition-shadow',
    assigneeSelected: 'ring-2 ring-primary-800',
    unassignedAvatar: 'flex h-9 w-9 items-center justify-center rounded-full border border-border ' +
        'bg-surface-overlay text-sm font-medium text-text-muted',
    dateInput: 'w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    dateOverdue: 'mb-5 text-sm font-semibold text-primary-700',
    fieldError: 'mt-1 mb-3 text-xs text-primary-700',
    footer: 'border-t border-border px-6 py-4',
    footerButtons: 'flex gap-3',
    cancelButton: 'flex-1 rounded-lg border border-border py-2 text-sm font-medium text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-text',
    saveButton: 'flex-1 rounded-lg bg-primary-800 py-2 text-sm font-medium text-text-inverted ' +
        'transition-colors hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
});
