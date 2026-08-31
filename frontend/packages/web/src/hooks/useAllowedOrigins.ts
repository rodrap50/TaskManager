import { useCallback, useEffect, useState } from 'react';
import { getAllowedOrigins, type AllowedOriginDto } from '@taskmanager/shared';

export function useAllowedOrigins() {
    const [data, setData]         = useState<AllowedOriginDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            setData(await getAllowedOrigins());
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load allowed origins');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
