export const useModalStyles = () => ({
    // Bottom sheet on a phone, centred dialog from `sm` up — the same treatment
    // CreateProjectModal already uses. z-[60] clears the fixed FloatingNav
    // (z-50), which otherwise paints over the footer buttons.
    overlay: 'fixed inset-0 z-[60] flex items-end justify-center bg-black/60 sm:items-center sm:p-4',
    // max-h + overflow so a tall form (TaskForm with the assignee picker open)
    // stays scrollable instead of running off the bottom of a short viewport.
    panel: 'max-h-[90dvh] w-full max-w-lg overflow-y-auto rounded-t-2xl border border-border bg-surface-overlay sm:rounded-xl ' +
        'animate-[modalIn_180ms_ease-out,panelGlow_3s_ease-in-out_infinite] ' +
        'motion-reduce:animate-none motion-reduce:shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]',
    heading: 'px-4 pt-5 pb-2 text-lg font-semibold text-text sm:px-6',
    body: 'px-4 pb-6 sm:px-6',
    footer: 'flex gap-3 border-t border-border px-4 pt-4 pb-[calc(1rem+env(safe-area-inset-bottom))] sm:px-6 sm:pb-4',
});
