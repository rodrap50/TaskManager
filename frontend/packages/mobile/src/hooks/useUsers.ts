import { useCallback, useEffect, useState } from 'react';
import { dbService } from '../core/Database';
import { getUsers, type AppUserDto } from '@taskmanager/shared';
import { useSyncEngine } from '../features/sync/useSyncEngine';

export function useUsers() {
    const { isOnline } = useSyncEngine();
    const [data, setData]         = useState<AppUserDto[]>([]);
    const [isLoading, setLoading] = useState(true);
    const [error, setError]       = useState<string | null>(null);

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const db = dbService.getDb();

            if (db) {
                const local = await db.query(
                    `SELECT id, username, displayName, email, avatarUrl, isAdmin, isActive, createdAt
                     FROM users WHERE isActive = 1 ORDER BY displayName ASC`
                );
                setData((local.values ?? []).map(r => ({
                    id: r.id as string,
                    username: r.username as string,
                    displayName: r.displayName as string,
                    email: r.email as string,
                    avatarUrl: (r.avatarUrl as string | null) ?? null,
                    isActive: Boolean(r.isActive),
                    isAdmin: Boolean(r.isAdmin),
                    createdAt: r.createdAt as string,
                    updatedAt: null,
                })));
            }

            if (isOnline) {
                const remote = await getUsers();
                if (db) {
                    for (const u of remote) {
                        await db.run(
                            `INSERT INTO users (id, username, displayName, email, avatarUrl, isAdmin, isActive, createdAt)
                             VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                             ON CONFLICT(id) DO UPDATE SET
                                username = excluded.username, displayName = excluded.displayName,
                                email = excluded.email, avatarUrl = excluded.avatarUrl,
                                isAdmin = excluded.isAdmin, isActive = excluded.isActive`,
                            [u.id, u.username, u.displayName, u.email, u.avatarUrl,
                             u.isAdmin ? 1 : 0, u.isActive ? 1 : 0, u.createdAt]
                        );
                    }
                }
                setData(remote);
            }
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load users');
        } finally {
            setLoading(false);
        }
    }, [isOnline]);

    useEffect(() => { void load(); }, [load]);

    return { data, isLoading, error, refetch: load };
}
