import { useCallback, useEffect, useState } from 'react';
import { getProjects, type ProjectDto } from '@taskmanager/shared';

export function useProjects() {
    const [data, setData]         = useState<ProjectDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            setData(await getProjects());
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load projects');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
