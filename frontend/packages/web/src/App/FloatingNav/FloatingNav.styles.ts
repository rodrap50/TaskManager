export const useFloatingNavStyles = () => ({
    container: 'fixed bottom-4 left-1/2 z-50 flex w-[60vw] max-w-4xl -translate-x-1/2 items-center justify-between ' +
        'rounded-full border border-border bg-surface-raised/90 px-8 py-2 backdrop-blur-md ' +
        'animate-[pillGlow_5s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    tabBase: 'flex flex-col items-center gap-0.5 py-1 px-5 text-xs font-medium transition-colors',
    tabActive: 'text-primary-700',
    tabInactive: 'text-text-muted hover:text-text',
    icon: 'h-5 w-5',
});
