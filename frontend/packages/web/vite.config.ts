import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@taskmanager/shared': resolve(__dirname, '../shared/src'),
            '@taskmanager/ui': resolve(__dirname, '../ui/src'),
        },
    },
    server: {
        port: 5173,
    },
});
