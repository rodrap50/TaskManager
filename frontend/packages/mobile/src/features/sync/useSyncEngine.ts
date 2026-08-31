import { createContext, useContext, useEffect, useState } from 'react';
import axios from 'axios';
import { dbService } from '../../core/Database';
import { v4 as uuidv4 } from 'uuid';

export interface SyncContextType {
    isOnline: boolean;
    syncing: boolean;
    queueMutation: (entityType: string, entityId: string, action: 'CREATE' | 'UPDATE' | 'DELETE', payload: unknown) => Promise<void>;
    forceSync: () => Promise<void>;
}

export const SyncContext = createContext<SyncContextType | null>(null);

export const useSyncEngine = () => {
    const context = useContext(SyncContext);
    if (!context) throw new Error('useSyncEngine must be used within SyncProvider');
    return context;
};

export const useSyncEngineProvider = () => {
    const [isOnline, setIsOnline] = useState(navigator.onLine);
    const [syncing, setSyncing]   = useState(false);

    useEffect(() => {
        const handleOnline  = () => setIsOnline(true);
        const handleOffline = () => setIsOnline(false);
        window.addEventListener('online',  handleOnline);
        window.addEventListener('offline', handleOffline);
        return () => {
            window.removeEventListener('online',  handleOnline);
            window.removeEventListener('offline', handleOffline);
        };
    }, []);

    useEffect(() => {
        if (isOnline) void forceSync();
    }, [isOnline]);

    const queueMutation = async (entityType: string, entityId: string, action: 'CREATE' | 'UPDATE' | 'DELETE', payload: unknown) => {
        const db = dbService.getDb();
        if (!db) return;
        const id = uuidv4();
        await db.run(
            `INSERT INTO mutation_queue (id, entityId, entityType, action, payload, timestamp) VALUES (?, ?, ?, ?, ?, ?)`,
            [id, entityId, entityType, action, JSON.stringify(payload), new Date().toISOString()]
        );
        if (isOnline) void forceSync();
    };

    const forceSync = async () => {
        if (syncing || !isOnline) return;
        setSyncing(true);
        try {
            const db = dbService.getDb();
            if (!db) return;
            const result    = await db.query(`SELECT * FROM mutation_queue ORDER BY timestamp ASC`);
            const mutations = result.values ?? [];

            for (const mutation of mutations) {
                try {
                    const url     = `http://localhost:8080/api/${(mutation.entityType as string).toLowerCase()}`;
                    const payload = JSON.parse(mutation.payload as string) as unknown;

                    if (mutation.action === 'CREATE')       await axios.post(url, payload);
                    else if (mutation.action === 'UPDATE')  await axios.put(`${url}/${mutation.entityId as string}`, payload);
                    else if (mutation.action === 'DELETE')  await axios.delete(`${url}/${mutation.entityId as string}`);

                    await db.run(`DELETE FROM mutation_queue WHERE id = ?`, [mutation.id]);
                } catch (err) {
                    console.error('Failed to sync mutation:', mutation.id, err);
                    await db.run(`UPDATE mutation_queue SET retryCount = retryCount + 1 WHERE id = ?`, [mutation.id]);
                    break;
                }
            }
        } finally {
            setSyncing(false);
        }
    };

    return { isOnline, syncing, queueMutation, forceSync };
};
