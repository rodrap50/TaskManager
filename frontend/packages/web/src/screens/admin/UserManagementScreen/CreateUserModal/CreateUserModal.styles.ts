export const useCreateUserModalStyles = () => ({
    label: 'mb-1 block text-sm font-medium text-text-muted',
    input: 'mb-4 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    toggleRow: 'mb-4 flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2',
    toggleLabel: 'text-sm text-text',
    toggleHint: 'text-xs text-text-muted',
    toggleButtonBase: 'relative h-5 w-9 shrink-0 rounded-full transition-colors',
    toggleButtonOn: 'bg-primary-800 dark:bg-primary-700',
    toggleButtonOff: 'bg-surface-overlay',
    toggleKnob: 'absolute top-0.5 h-4 w-4 rounded-full bg-text-inverted transition-transform',
    toggleKnobOn: 'translate-x-4',
    toggleKnobOff: 'translate-x-0.5',
    errorBox: 'mb-4 rounded-md border border-primary-800/40 bg-primary-900/10 px-3 py-2 text-sm text-primary-700',
    actionsRow: 'flex gap-3 pt-2',
    cancelButton: 'flex-1 rounded-lg border border-border py-2 text-sm font-medium text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-text',
    submitButton: 'flex-1 rounded-lg bg-primary-800 py-2 text-sm font-medium text-text-inverted ' +
        'transition-colors hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
});
