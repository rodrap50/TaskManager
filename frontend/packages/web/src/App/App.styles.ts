export const useAppStyles = () => ({
    root: 'flex h-dvh flex-col overflow-hidden bg-surface',

    // FloatingPill and FloatingNav are both `fixed`, so <main> has to reserve
    // their space itself. The safe-area insets keep that clearance correct on
    // notched phones, where the pill/nav are pushed inwards by the same amount
    // (see index.html's viewport-fit=cover).
    main: 'flex-1 overflow-y-auto overflow-x-hidden ' +
        'pt-[calc(4.5rem+env(safe-area-inset-top))] sm:pt-[calc(5rem+env(safe-area-inset-top))] ' +
        'pb-[calc(6.5rem+env(safe-area-inset-bottom))] sm:pb-[calc(6rem+env(safe-area-inset-bottom))]',

    loading: 'flex h-dvh w-full items-center justify-center bg-surface text-sm text-text-muted',
});
