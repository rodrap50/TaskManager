export const useSetupScreenStyles = () => ({
    // min-h-dvh + scroll rather than h-dvh — see LoginScreen.styles.
    container: 'flex min-h-dvh w-full items-center justify-center overflow-y-auto bg-surface px-4 py-8',
    panel: 'w-full max-w-sm rounded-2xl border border-border bg-surface-overlay p-6 ' +
        'animate-[panelGlow_3s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    brandMark: 'mb-6 text-center text-sm font-semibold tracking-wide text-text-muted',
    heading: 'mb-1 text-lg font-semibold text-text',
    subheading: 'mb-6 text-sm text-text-muted',
    label: 'mb-1 block text-sm font-medium text-text-muted',
    input: 'mb-4 w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text ' +
        'focus:border-primary-700 focus:outline-none',
    hint: 'mb-4 -mt-3 text-xs text-primary-700',
    errorBox: 'mb-4 rounded-md border border-primary-800/40 bg-primary-900/10 px-3 py-2 text-sm text-primary-700',
    submitButton: 'min-h-11 w-full rounded-lg bg-primary-800 py-2 text-sm font-medium text-text-inverted ' +
        'transition-colors hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50 sm:min-h-0',
});
