import React, { type ReactNode } from 'react';
import { SyncContext, useSyncEngineProvider } from './useSyncEngine';

export const SyncProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const syncEngine = useSyncEngineProvider();
    return (
        <SyncContext.Provider value={syncEngine}>
            {children}
        </SyncContext.Provider>
    );
};
