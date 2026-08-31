import { useCallback, useEffect, useState } from 'react';
import { dbService } from '../core/Database';
import { getPhasesByProject, type ProjectPhaseDto } from '@taskmanager/shared';
import { useSyncEngine } from '../features/sync/useSyncEngine';

export function usePhases(projectId: string | null) {
    const { isOnline } = useSyncEngine();
    const [data, setData]         = useState<ProjectPhaseDto[]>([]);
    const [isLoading, setLoading] = useState(false);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        if (!projectId) { setData([]); return; }
        setLoading(true);
        setError(null);
        try {
            const db = dbService.getDb();

            if (db) {
                const local = await db.query(
                    `SELECT id, projectId, name, description, sortOrder as displayOrder, createdAt, updatedAt
                     FROM phases WHERE projectId = ? ORDER BY sortOrder ASC`,
                    [projectId]
                );
                setData((local.values ?? []).map(r => ({
                    id: r.id as string,
                    projectId: r.projectId as string,
                    name: r.name as string,
                    description: (r.description as string | null) ?? null,
                    displayOrder: r.displayOrder as number,
                    createdAt: r.createdAt as string,
                    updatedAt: (r.updatedAt as string | null) ?? null,
                })));
            }

            if (isOnline) {
                const remote = await getPhasesByProject(projectId);
                if (db) {
                    for (const ph of remote) {
                        await db.run(
                            `INSERT INTO phases (id, projectId, name, description, sortOrder, createdAt, updatedAt)
                             VALUES (?, ?, ?, ?, ?, ?, ?)
                             ON CONFLICT(id) DO UPDATE SET
                                name = excluded.name, description = excluded.description,
                                sortOrder = excluded.sortOrder, updatedAt = excluded.updatedAt`,
                            [ph.id, ph.projectId, ph.name, ph.description, ph.displayOrder, ph.createdAt, ph.updatedAt]
                        );
                    }
                }
                setData(remote);
            }
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load phases');
        } finally {
            setLoading(false);
        }
    }, [projectId, isOnline]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
