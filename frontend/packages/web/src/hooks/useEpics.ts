import { useCallback, useEffect, useState } from 'react';
import { getEpicsByProject, type EpicDto } from '@taskmanager/shared';

export function useEpics(projectId: string | null) {
    const [data, setData]         = useState<EpicDto[]>([]);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData([]); return; }
        setLoading(true);
        setError(null);
        try {
            setData(await getEpicsByProject(projectId));
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load epics');
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
