import { useCallback, useEffect, useState } from 'react';
import { dbService } from '../core/Database';
import { getProjects, type ProjectDto } from '@taskmanager/shared';
import { useSyncEngine } from '../features/sync/useSyncEngine';

export function useProjects() {
    const { isOnline } = useSyncEngine();
    const [data, setData]         = useState<ProjectDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const db = dbService.getDb();

            if (db) {
                const local = await db.query(
                    `SELECT id, name, description, scope, colorHex, dueDate, isArchived,
                            createdByUserId, createdAt, updatedAt
                     FROM projects WHERE isArchived = 0 ORDER BY createdAt DESC`
                );
                const localRows: ProjectDto[] = (local.values ?? []).map(r => ({
                    id: r.id as string,
                    name: r.name as string,
                    description: (r.description as string | null) ?? null,
                    scope: (r.scope as string) ?? 'Project',
                    colorHex: (r.colorHex as string | null) ?? null,
                    dueDate: (r.dueDate as string | null) ?? null,
                    isArchived: Boolean(r.isArchived),
                    createdByUserId: (r.createdByUserId as string) ?? '',
                    createdAt: r.createdAt as string,
                    updatedAt: (r.updatedAt as string | null) ?? null,
                    phases: [],
                    tasks: [],
                }));
                setData(localRows);
            }

            if (isOnline) {
                const remote = await getProjects();
                if (db) {
                    for (const p of remote) {
                        await db.run(
                            `INSERT INTO projects
                                (id, name, description, scope, colorHex, dueDate, isArchived, createdByUserId, createdAt, updatedAt, version, isDeleted)
                             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, 0)
                             ON CONFLICT(id) DO UPDATE SET
                                name = excluded.name, description = excluded.description,
                                scope = excluded.scope, colorHex = excluded.colorHex,
                                dueDate = excluded.dueDate, isArchived = excluded.isArchived,
                                createdByUserId = excluded.createdByUserId, updatedAt = excluded.updatedAt`,
                            [p.id, p.name, p.description, p.scope, p.colorHex, p.dueDate,
                             p.isArchived ? 1 : 0, p.createdByUserId, p.createdAt, p.updatedAt]
                        );
                    }
                }
                setData(remote);
            }
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load projects');
        } finally {
            setLoading(false);
        }
    }, [isOnline]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
