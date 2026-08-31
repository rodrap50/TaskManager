import { CapacitorSQLite, SQLiteConnection, SQLiteDBConnection } from '@capacitor-community/sqlite';
import { Capacitor } from '@capacitor/core';
import { defineCustomElements as jeepSqlite } from 'jeep-sqlite/loader';

const DB_NAME = 'taskmanager_db';
const DB_VERSION = 2;

const SCHEMA_V1 = `
    CREATE TABLE IF NOT EXISTS projects (
        id          TEXT PRIMARY KEY NOT NULL,
        name        TEXT NOT NULL,
        description TEXT,
        createdAt   TEXT NOT NULL,
        updatedAt   TEXT,
        version     INTEGER NOT NULL,
        isDeleted   INTEGER DEFAULT 0
    );

    CREATE TABLE IF NOT EXISTS mutation_queue (
        id         TEXT PRIMARY KEY NOT NULL,
        entityId   TEXT NOT NULL,
        entityType TEXT NOT NULL,
        action     TEXT NOT NULL,
        payload    TEXT NOT NULL,
        timestamp  TEXT NOT NULL,
        retryCount INTEGER DEFAULT 0
    );
`;

const UPGRADE_V1_TO_V2 = [
    `ALTER TABLE projects ADD COLUMN scope INTEGER DEFAULT 1;`,
    `ALTER TABLE projects ADD COLUMN colorHex TEXT;`,
    `ALTER TABLE projects ADD COLUMN isArchived INTEGER DEFAULT 0;`,
    `ALTER TABLE projects ADD COLUMN createdByUserId TEXT;`,

    `CREATE TABLE IF NOT EXISTS tasks (
        id                   TEXT PRIMARY KEY NOT NULL,
        projectId            TEXT NOT NULL,
        phaseId              TEXT,
        title                TEXT NOT NULL,
        description          TEXT,
        status               INTEGER NOT NULL DEFAULT 0,
        priority             INTEGER NOT NULL DEFAULT 1,
        assignedUserId       TEXT,
        secondaryAssigneeId  TEXT,
        dueDate              TEXT,
        estimatedHours       REAL,
        externalMetadata     TEXT,
        createdAt            TEXT NOT NULL,
        updatedAt            TEXT
    );`,

    `CREATE INDEX IF NOT EXISTS idx_tasks_project ON tasks (projectId);`,
    `CREATE INDEX IF NOT EXISTS idx_tasks_phase   ON tasks (phaseId);`,

    `CREATE TABLE IF NOT EXISTS phases (
        id          TEXT PRIMARY KEY NOT NULL,
        projectId   TEXT NOT NULL,
        name        TEXT NOT NULL,
        description TEXT,
        sortOrder   INTEGER NOT NULL DEFAULT 0,
        createdAt   TEXT NOT NULL,
        updatedAt   TEXT
    );`,

    `CREATE INDEX IF NOT EXISTS idx_phases_project ON phases (projectId);`,

    `CREATE TABLE IF NOT EXISTS users (
        id          TEXT PRIMARY KEY NOT NULL,
        username    TEXT NOT NULL,
        displayName TEXT NOT NULL,
        email       TEXT NOT NULL,
        avatarUrl   TEXT,
        isAdmin     INTEGER NOT NULL DEFAULT 0,
        isActive    INTEGER NOT NULL DEFAULT 1,
        createdAt   TEXT NOT NULL
    );`,
];

export class DatabaseService {
    private sqlite: SQLiteConnection;
    private db!: SQLiteDBConnection;
    isAvailable = false;

    constructor() {
        this.sqlite = new SQLiteConnection(CapacitorSQLite);
    }

    async init() {
        try {
            if (Capacitor.getPlatform() === 'web') {
                jeepSqlite(window);
                const jeepEl = document.createElement('jeep-sqlite');
                document.body.appendChild(jeepEl);
                await customElements.whenDefined('jeep-sqlite');
                await this.sqlite.initWebStore();
            }

            await this.sqlite.addUpgradeStatement(DB_NAME, [
                { toVersion: 1, statements: [SCHEMA_V1] },
                { toVersion: 2, statements: UPGRADE_V1_TO_V2 },
            ]);

            const ret    = await this.sqlite.checkConnectionsConsistency();
            const isConn = (await this.sqlite.isConnection(DB_NAME, false)).result;

            if (ret.result && isConn) {
                this.db = await this.sqlite.retrieveConnection(DB_NAME, false);
            } else {
                this.db = await this.sqlite.createConnection(DB_NAME, false, 'no-encryption', DB_VERSION, false);
            }

            await this.db.open();

            if (Capacitor.getPlatform() === 'web') {
                await this.sqlite.saveToStore(DB_NAME);
            }

            this.isAvailable = true;
        } catch (err) {
            console.warn('[DatabaseService] SQLite unavailable, falling back to API-only mode:', err);
            this.isAvailable = false;
        }
    }

    getDb(): SQLiteDBConnection | null {
        return this.isAvailable ? this.db : null;
    }
}

export const dbService = new DatabaseService();
