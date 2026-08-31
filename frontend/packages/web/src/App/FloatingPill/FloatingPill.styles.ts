export const useFloatingPillStyles = () => ({
    container: 'fixed top-4 right-4 z-50 flex items-center gap-1.5 overflow-hidden rounded-full border border-border ' +
        'bg-surface-raised/90 backdrop-blur-md px-3 py-2 ' +
        'animate-[pillGlow_5s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    spotlight: 'pointer-events-none absolute inset-0 rounded-full transition-opacity duration-300',
    brandMark: 'h-2 w-2 shrink-0 rounded-full bg-primary-700',
    divider: 'h-4 w-px shrink-0 bg-border',
    iconButton: 'flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-text-muted transition-colors hover:text-text',
    iconSvg: 'h-3.5 w-3.5',
});
