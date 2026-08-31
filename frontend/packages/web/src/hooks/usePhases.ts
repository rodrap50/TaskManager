import { useCallback, useEffect, useState } from 'react';
import { getPhasesByProject, type ProjectPhaseDto } from '@taskmanager/shared';

export function usePhases(projectId: string | null) {
    const [data, setData]         = useState<ProjectPhaseDto[]>([]);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData([]); return; }
        setLoading(true);
        setError(null);
        try {
            setData(await getPhasesByProject(projectId));
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load phases');
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
