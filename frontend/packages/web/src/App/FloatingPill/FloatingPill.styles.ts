export const useFloatingPillStyles = () => ({
    container: 'fixed top-[calc(0.75rem+env(safe-area-inset-top))] right-3 sm:top-[calc(1rem+env(safe-area-inset-top))] sm:right-4 ' +
        'z-50 flex items-center gap-1.5 overflow-hidden rounded-full border border-border ' +
        'bg-surface-raised/90 backdrop-blur-md px-3 py-2 ' +
        'animate-[pillGlow_5s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    spotlight: 'pointer-events-none absolute inset-0 rounded-full transition-opacity duration-300',
    brandMark: 'h-2 w-2 shrink-0 rounded-full bg-primary-700',
    divider: 'h-4 w-px shrink-0 bg-border',
    // 24px is a comfortable mouse target but a miss-prone one for a thumb, so
    // the hit area grows to 36px on touch-sized screens while the glyph stays put.
    iconButton: 'flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-text-muted ' +
        'transition-colors hover:text-text sm:h-6 sm:w-6',
    iconSvg: 'h-4 w-4 sm:h-3.5 sm:w-3.5',
});
