import { useCallback, useEffect, useState } from 'react';
import { getProjectMembers, type AppUserDto } from '@taskmanager/shared';

export function useProjectMembers(projectId: string | null) {
    const [data, setData]         = useState<AppUserDto[]>([]);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData([]); return; }
        setLoading(true);
        setError(null);
        try {
            setData(await getProjectMembers(projectId));
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load members');
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
