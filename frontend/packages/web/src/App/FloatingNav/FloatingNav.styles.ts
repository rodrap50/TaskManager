export const useFloatingNavStyles = () => ({
    // 60vw reads as a centred toolbar on a desktop monitor but as a cramped stub
    // on a phone, so below `sm` the bar spans the viewport (less a 12px gutter
    // each side) and the tabs split it evenly via flex-1.
    container: 'fixed bottom-[calc(1rem+env(safe-area-inset-bottom))] left-1/2 z-50 flex ' +
        'w-[calc(100%-1.5rem)] max-w-sm sm:w-[60vw] sm:max-w-4xl -translate-x-1/2 items-center justify-between ' +
        'rounded-full border border-border bg-surface-raised/90 px-2 sm:px-8 py-2 backdrop-blur-md ' +
        'animate-[pillGlow_5s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    tabBase: 'flex min-h-11 flex-1 flex-col items-center justify-center gap-0.5 rounded-full px-2 py-1 ' +
        'text-xs font-medium transition-colors sm:min-h-0 sm:flex-initial sm:px-5',
    tabActive: 'text-primary-700',
    tabInactive: 'text-text-muted hover:text-text',
    icon: 'h-5 w-5',
});
