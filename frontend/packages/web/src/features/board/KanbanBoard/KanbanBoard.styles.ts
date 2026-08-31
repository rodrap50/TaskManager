export const useKanbanBoardStyles = () => ({
    board: 'flex h-full gap-6 overflow-x-auto p-4 pb-6',
    columnBase: 'flex w-64 flex-shrink-0 flex-col rounded-xl border-2 shadow-[0_8px_24px_-8px_rgba(0,0,0,0.4)] transition-colors',
    columnIdle: 'border-border bg-surface-raised',
    columnOver: 'border-primary-700 bg-primary-900/10',
    columnInvalid: 'border-border bg-surface-raised opacity-40 cursor-not-allowed',
    header: 'flex items-center justify-between border-b border-primary-700 px-3 py-2.5 shadow-[0_4px_12px_-2px_rgba(185,28,28,0.5)] [clip-path:inset(0_0_-20px_0)]',
    headerLabel: 'text-sm font-semibold text-text',
    countBadge: 'rounded-full bg-surface-overlay px-1.5 py-0.5 text-[11px] font-medium text-text-muted',
    taskList: 'flex flex-1 flex-col gap-2 overflow-y-auto px-2 py-3',
    taskWrapBase: 'transition-opacity',
    taskDragging: 'opacity-40',
    taskIdle: 'opacity-100',
    emptyBase: 'flex h-16 items-center justify-center rounded-md border border-dashed text-center text-xs text-text-muted',
    emptyIdle: 'border-border-subtle',
    emptyOver: 'border-primary-700 text-primary-700',
    // Top-center placement (clear of FloatingPill's top-4 right-4 corner), same
    // rounded/blur look as FloatingPill and FloatingNav — including their static
    // crimson glow shadow, since a plain shadow-lg barely reads against the dark
    // surface tones (that's why the shadow seemed to vanish once pillGlow's own
    // animated box-shadow — which had been overriding shadow-lg the whole time —
    // was swapped out for the opacity-only toastFade animation).
    // 15s hold-then-fade is timed to match the setTimeout that unmounts it in KanbanBoard.tsx;
    // the X button lets the user dismiss it (and cut the animation short) before then.
    // Below `sm`: message gets forced onto its own line via basis-full, and close is
    // reordered ahead of it (order-1 vs order-2) so title+close land together on row 1
    // while message wraps to row 2 — divider hidden and shape relaxed to rounded-2xl
    // since a two-line layout doesn't read well as a full pill anymore.
    toast: 'fixed top-8 left-1/2 z-50 -translate-x-1/2 flex max-w-[90vw] flex-wrap items-center gap-x-2 gap-y-1.5 ' +
        'rounded-full max-sm:justify-between max-sm:rounded-2xl border border-border bg-surface-raised/90 ' +
        'backdrop-blur-md px-4 py-2 text-sm text-text shadow-[0_0_14px_3px_rgba(185,28,28,0.3)] ' +
        'animate-[toastFade_15s_ease-out_forwards] motion-reduce:animate-none',
    toastTitle: 'flex-shrink-0 font-semibold text-primary-700',
    toastDivider: 'h-4 w-px flex-shrink-0 bg-border max-sm:hidden',
    toastMessage: 'text-text max-sm:order-2 max-sm:basis-full',
    toastClose: 'flex-shrink-0 text-text-muted transition-colors hover:text-text max-sm:order-1',
    toastCloseIcon: 'h-3.5 w-3.5',
});
