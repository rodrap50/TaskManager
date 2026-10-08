import { useCallback, useEffect, useState } from 'react';
import { getApiTokens, type ApiTokenDto } from '@taskmanager/shared';

export function useApiTokens() {
    const [data, setData]         = useState<ApiTokenDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            setData(await getApiTokens());
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load API tokens');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
