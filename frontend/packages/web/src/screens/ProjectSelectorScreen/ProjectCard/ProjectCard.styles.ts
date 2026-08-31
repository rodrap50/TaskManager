export const useProjectCardStyles = () => ({
    card: 'group relative flex w-[80dvw] max-w-6xl items-start gap-3 p-4 overflow-hidden rounded-xl border border-border ' +
        'bg-surface-raised py-4 pl-5 pr-4 text-left transition-all ' +
        'shadow-[0_4px_16px_-2px_rgba(185,28,28,0.25)] ' +
        'hover:shadow-[0_8px_28px_-4px_rgba(185,28,28,0.35)] hover:-translate-y-px hover:border-border-subtle active:scale-[0.99]',
    accentBar: 'absolute inset-y-0 left-0 w-1',
    body: 'min-w-0',
    name: 'truncate font-medium text-text',
    description: 'mt-0.5 truncate text-sm text-text-muted',
    meta: 'mt-1 text-xs text-text-muted',
});
