export const useCreateProjectModalStyles = () => ({
    overlay: 'fixed inset-0 z-50 flex items-end justify-center bg-black/50 backdrop-blur-sm sm:items-center',
    panel: 'w-full max-w-md rounded-t-2xl border border-border bg-surface-overlay p-6 sm:rounded-2xl ' +
        'animate-[panelGlow_3s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    heading: 'mb-4 text-lg font-semibold text-text',
    label: 'mb-1 block text-sm font-medium text-text-muted',
    input: 'mb-4 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    textarea: 'mb-4 w-full resize-none rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    typeRow: 'mb-4 flex gap-2',
    typeButtonBase: 'flex-1 rounded-lg border py-1.5 text-sm font-medium transition-colors',
    typeButtonActive: 'border-transparent bg-primary-900/20 text-primary-700 ' +
        'animate-[swatchGlow_1.8s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_0_2px_var(--color-primary-700)]',
    typeButtonInactive: 'border-border text-text-muted hover:border-border-subtle hover:text-text',
    colorRow: 'mt-3 mb-6 flex flex-wrap gap-2.5',
    colorSwatchBase: 'h-7 w-7 overflow-hidden rounded-full transition-transform',
    colorSwatchActive: 'animate-[swatchGlow_1.8s_ease-in-out_infinite] ' +
        'motion-reduce:animate-none motion-reduce:shadow-[0_0_0_2px_var(--color-primary-700)]',
    colorSwatchInactive: 'hover:scale-110',
    colorSwatchFill: 'color-swatch block h-full w-full',
    errorBox: 'mb-4 rounded-md border border-primary-800/40 bg-primary-900/10 px-3 py-2 text-sm text-primary-700',
    actionsRow: 'flex gap-3',
    cancelButton: 'flex-1 rounded-lg border border-border py-2 text-sm font-medium text-text-muted ' +
        'transition-colors hover:bg-surface hover:text-text',
    submitButton: 'flex-1 rounded-lg bg-primary-800 py-2 text-sm font-medium text-text-inverted ' +
        'transition-colors hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50',
});
