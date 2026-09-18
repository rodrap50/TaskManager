export const useVoteControlStyles = () => ({
    labelRow: 'mb-1.5 flex items-baseline justify-between gap-2',
    sectionLabel: 'text-xs font-medium uppercase tracking-wide text-text-muted',
    aggregate: 'text-xs font-medium text-text-muted',
    pillRow: 'mb-5 flex flex-wrap gap-2',
    pillBase: 'h-9 w-9 rounded-full border text-xs font-medium transition-colors disabled:cursor-not-allowed ' +
        'disabled:opacity-50 sm:h-7 sm:w-7',
    pillActive: 'border-transparent bg-primary-800 text-text-inverted',
    pillInactive: 'border-border text-text-muted hover:border-border-subtle hover:text-text',
    fieldError: 'mt-1 mb-3 text-xs text-primary-700',
});
