import { useCallback, useEffect, useState } from 'react';
import { getUsers, type AppUserDto } from '@taskmanager/shared';

export function useUsers() {
    const [data, setData]         = useState<AppUserDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            setData(await getUsers());
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load users');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
