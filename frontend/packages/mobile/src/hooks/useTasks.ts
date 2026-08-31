import { useCallback, useEffect, useState } from 'react';
import { dbService } from '../core/Database';
import { getTasksByProject, type ProjectTaskDto } from '@taskmanager/shared';
import { useSyncEngine } from '../features/sync/useSyncEngine';

function statusLabel(n: number): string {
    return ['Backlog', 'InProgress', 'Blocked', 'Done', 'Cancelled'][n] ?? 'Backlog';
}
function priorityLabel(n: number): string {
    return ['Low', 'Medium', 'High', 'Critical'][n] ?? 'Medium';
}
function statusOrdinal(s: string): number {
    const map: Record<string, number> = { Backlog: 0, InProgress: 1, Blocked: 2, Done: 3, Cancelled: 4 };
    return map[s] ?? 0;
}
function priorityOrdinal(p: string): number {
    const map: Record<string, number> = { Low: 0, Medium: 1, High: 2, Critical: 3 };
    return map[p] ?? 1;
}

export function useTasks(projectId: string | null) {
    const { isOnline } = useSyncEngine();
    const [data, setData]         = useState<ProjectTaskDto[]>([]);
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
                    `SELECT id, projectId, phaseId, title, description, status, priority,
                            assignedUserId, secondaryAssigneeId, dueDate, estimatedHours,
                            externalMetadata, createdAt, updatedAt
                     FROM tasks WHERE projectId = ? ORDER BY priority DESC, dueDate ASC`,
                    [projectId]
                );
                setData((local.values ?? []).map(r => ({
                    id: r.id as string,
                    projectId: r.projectId as string,
                    phaseId: (r.phaseId as string | null) ?? null,
                    assignedUserId: (r.assignedUserId as string | null) ?? null,
                    secondaryAssigneeId: (r.secondaryAssigneeId as string | null) ?? null,
                    title: r.title as string,
                    description: (r.description as string | null) ?? null,
                    status: statusLabel(r.status as number),
                    priority: priorityLabel(r.priority as number),
                    dueDate: (r.dueDate as string | null) ?? null,
                    estimatedHours: (r.estimatedHours as number | null) ?? null,
                    isCompleted: r.status === 3,
                    externalMetadata: r.externalMetadata ? JSON.parse(r.externalMetadata as string) as Record<string, string> : {},
                    createdAt: r.createdAt as string,
                    updatedAt: (r.updatedAt as string | null) ?? null,
                })));
            }

            if (isOnline) {
                const remote = await getTasksByProject(projectId);
                if (db) {
                    for (const t of remote) {
                        await db.run(
                            `INSERT INTO tasks
                                (id, projectId, phaseId, title, description, status, priority,
                                 assignedUserId, secondaryAssigneeId, dueDate, estimatedHours,
                                 externalMetadata, createdAt, updatedAt)
                             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                             ON CONFLICT(id) DO UPDATE SET
                                phaseId = excluded.phaseId, title = excluded.title,
                                description = excluded.description, status = excluded.status,
                                priority = excluded.priority, assignedUserId = excluded.assignedUserId,
                                secondaryAssigneeId = excluded.secondaryAssigneeId,
                                dueDate = excluded.dueDate, estimatedHours = excluded.estimatedHours,
                                externalMetadata = excluded.externalMetadata, updatedAt = excluded.updatedAt`,
                            [t.id, t.projectId, t.phaseId, t.title, t.description,
                             statusOrdinal(t.status), priorityOrdinal(t.priority),
                             t.assignedUserId, t.secondaryAssigneeId, t.dueDate,
                             t.estimatedHours, JSON.stringify(t.externalMetadata),
                             t.createdAt, t.updatedAt]
                        );
                    }
                }
                setData(remote);
            }
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load tasks');
        } finally {
            setLoading(false);
        }
    }, [projectId, isOnline]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
