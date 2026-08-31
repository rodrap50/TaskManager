import { useCallback, useEffect, useState } from 'react';
import { getProjectById, type ProjectDto } from '@taskmanager/shared';

export function useProjectDetail(projectId: string | null) {
    const [data, setData]         = useState<ProjectDto | null>(null);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData(null); return; }
        setLoading(true);
        setError(null);
        try {
            setData(await getProjectById(projectId));
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load project');
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
