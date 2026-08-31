export const useModalStyles = () => ({
    overlay: 'fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4',
    panel: 'w-full max-w-lg rounded-xl border border-border bg-surface-overlay ' +
        'animate-[modalIn_180ms_ease-out,panelGlow_3s_ease-in-out_infinite] ' +
        'motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    heading: 'px-6 pt-5 pb-2 text-lg font-semibold text-text',
    body: 'px-6 pb-6',
    footer: 'flex gap-3 border-t border-border px-6 py-4',
});
