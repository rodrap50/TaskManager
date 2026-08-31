export const useTaskCardStyles = () => ({
    card: 'relative cursor-pointer rounded-md border border-border bg-surface-overlay p-3 text-left transition-all duration-500 ease-out ' +
        'hover:shadow-[0_8px_28px_-4px_rgba(185,28,28,0.35)] hover:-translate-y-px hover:border-border-subtle active:scale-[0.99]',
    epicTag: 'absolute right-2 top-2 max-w-[45%] truncate rounded px-1.5 py-0.5 text-[11px] font-medium text-white shadow-sm',
    title: 'mt-2 line-clamp-2 text-sm font-medium text-text',
    footer: 'mt-2 flex items-center justify-between gap-2',
    footerRight: 'flex items-center gap-2',
    dueNormal: 'text-xs text-text-muted',
    dueOverdue: 'text-xs font-semibold text-primary-700',
});
