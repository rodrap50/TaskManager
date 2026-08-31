import { useCallback, useEffect, useState } from 'react';
import { getTasksByProject, type ProjectTaskDto } from '@taskmanager/shared';

export function useTasks(projectId: string | null) {
    const [data, setData]         = useState<ProjectTaskDto[]>([]);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData([]); return; }
        setLoading(true);
        setError(null);
        try {
            setData(await getTasksByProject(projectId));
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load tasks');
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
