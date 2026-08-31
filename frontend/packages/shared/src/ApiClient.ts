import axios from 'axios';

const BASE_URL = (import.meta as { env: Record<string, string> }).env.VITE_API_URL ?? 'http://localhost:5000';

/**
 * Resolves a server-relative asset path (e.g. an `AppUserDto.avatarUrl` of `/avatars/{id}.png`)
 * into an absolute URL the browser can fetch directly — the API and the Vite dev server run on
 * different origins, so a bare relative path would otherwise resolve against the frontend's own
 * origin instead of the backend's. Already-absolute URLs are returned unchanged. Nullable input
 * (e.g. a user with no avatar yet) passes through as `null` so call sites don't need a ternary.
 */
export function resolveAssetUrl(path: string): string;
export function resolveAssetUrl(path: string | null | undefined): string | null;
export function resolveAssetUrl(path: string | null | undefined): string | null {
    if (!path) return null;
    return /^https?:\/\//i.test(path) ? path : `${BASE_URL}${path}`;
}

// ---------------------------------------------------------------------------
// Response DTOs — mirror the .NET API records
// ---------------------------------------------------------------------------

export interface ProjectDto {
    id: string;
    name: string;
    description: string | null;
    scope: string; // 'Project' | 'DailyTask'
    colorHex: string | null;
    dueDate: string | null;
    isArchived: boolean;
    criticalityScore: number; // 1-10, admin-settable (PRI01.4), defaults to 5
    createdByUserId: string;
    createdAt: string;
    updatedAt: string | null;
    phases: ProjectPhaseDto[];
    epics: EpicDto[];
    tasks: ProjectTaskDto[];
}

export interface ProjectTaskDto {
    id: string;
    projectId: string;
    phaseId: string | null;
    epicId: string | null;
    assignedUserId: string | null;
    secondaryAssigneeId: string | null;
    title: string;
    description: string | null;
    status: string; // 'Backlog' | 'InProgress' | 'Blocked' | 'Done' | 'Cancelled'
    priority: string; // 'Low' | 'Medium' | 'High' | 'Critical'
    weightedScore: number;
    votes: Record<string, number>; // userId -> voteValue (1-10); look up the current user's own ID to find "my vote"
    dueDate: string | null;
    estimatedHours: number | null;
    isCompleted: boolean;
    externalMetadata: Record<string, string>;
    createdAt: string;
    updatedAt: string | null;
}

export interface ProjectPhaseDto {
    id: string;
    projectId: string;
    name: string;
    description: string | null;
    displayOrder: number;
    createdAt: string;
    updatedAt: string | null;
}

export interface EpicDto {
    id: string;
    projectId: string;
    name: string;
    description: string | null;
    colorHex: string | null;
    displayOrder: number;
    createdAt: string;
    updatedAt: string | null;
}

export interface AppUserDto {
    id: string;
    username: string;
    displayName: string;
    email: string;
    avatarUrl: string | null;
    isActive: boolean;
    isAdmin: boolean;
    createdAt: string;
    updatedAt: string | null;
}

export interface AllowedOriginDto {
    id: string;
    originUrl: string;
    createdAt: string;
}

// ---------------------------------------------------------------------------
// Request shapes
// ---------------------------------------------------------------------------

export interface CreateProjectRequest {
    name: string;
    description?: string;
    scope: 'Project' | 'DailyTask';
    colorHex?: string;
    dueDate?: string;
}

export interface UpdateProjectRequest {
    name?: string;
    description?: string;
    scope?: 'Project' | 'DailyTask';
    colorHex?: string;
    dueDate?: string;
}

export interface CreateTaskRequest {
    projectId: string;
    title: string;
    description?: string;
    priority: 'Low' | 'Medium' | 'High' | 'Critical';
    phaseId?: string;
    epicId?: string;
    assignedUserId?: string;
    secondaryAssigneeId?: string;
    dueDate?: string;
    estimatedHours?: number;
}

export interface UpdateTaskRequest {
    title?: string;
    description?: string;
    priority?: 'Low' | 'Medium' | 'High' | 'Critical';
    phaseId?: string;
    clearPhase?: boolean;
    epicId?: string;
    clearEpic?: boolean;
    assignedUserId?: string;
    clearAssignee?: boolean;
    secondaryAssigneeId?: string;
    clearSecondaryAssignee?: boolean;
    dueDate?: string;
    estimatedHours?: number;
}

export type TaskStatus = 'Backlog' | 'InProgress' | 'Blocked' | 'Done' | 'Cancelled';

export interface CreatePhaseRequest {
    projectId: string;
    name: string;
    description?: string;
    displayOrder?: number;
}

export interface UpdatePhaseRequest {
    name?: string;
    description?: string;
    displayOrder?: number;
}

export interface CreateEpicRequest {
    projectId: string;
    name: string;
    description?: string;
    colorHex?: string;
    displayOrder?: number;
}

export interface UpdateEpicRequest {
    name?: string;
    description?: string;
    displayOrder?: number;
}

export interface CreateUserRequest {
    username: string;
    displayName: string;
    email: string;
    password: string;
    isAdmin?: boolean;
}

export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    user: AppUserDto;
}

// ---------------------------------------------------------------------------
// Axios instance
// ---------------------------------------------------------------------------

const http = axios.create({
    baseURL: BASE_URL,
    headers: { 'Content-Type': 'application/json' },
    timeout: 10_000,
});

// ---------------------------------------------------------------------------
// Auth token storage — attached to every outgoing request automatically
// ---------------------------------------------------------------------------

const TOKEN_KEY = 'tm:token';

export const getStoredToken = () => localStorage.getItem(TOKEN_KEY);
export const setStoredToken = (token: string) => localStorage.setItem(TOKEN_KEY, token);
export const clearStoredToken = () => localStorage.removeItem(TOKEN_KEY);

http.interceptors.request.use(config => {
    const token = getStoredToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// A 401 from the login endpoint itself just means "wrong credentials" — not a
// session going invalid — so it's excluded from this broadcast.
export const UNAUTHORIZED_EVENT = 'tm:unauthorized';

http.interceptors.response.use(
    response => response,
    (error: unknown) => {
        if (
            axios.isAxiosError(error) &&
            error.response?.status === 401 &&
            !error.config?.url?.includes('/api/auth/login')
        ) {
            window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
        }
        return Promise.reject(error);
    },
);

// ---------------------------------------------------------------------------
// Auth
// ---------------------------------------------------------------------------

export const login = (body: LoginRequest) =>
    http.post<LoginResponse>('/api/auth/login', body).then(r => r.data);

export interface SetupStatusResponse {
    setupRequired: boolean;
}

export const getSetupStatus = () =>
    http.get<SetupStatusResponse>('/api/auth/setup/status').then(r => r.data);

export const setupAdmin = (body: LoginRequest) =>
    http.post<LoginResponse>('/api/auth/setup', body).then(r => r.data);

/** True if `error` is an axios error carrying an HTTP 409 (Conflict) response. */
export const isConflictError = (error: unknown): boolean =>
    axios.isAxiosError(error) && error.response?.status === 409;

/**
 * Extracts a human-readable message from a failed API call, preferring the server's own
 * `{ error }` (422 business-rule rejections) or `{ errors: [...] }` (400 validation
 * failures) response body over axios's generic "Request failed with status code N".
 */
export const getErrorMessage = (error: unknown, fallback: string): string => {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data as
            | { error?: string; errors?: { errorMessage: string }[] }
            | undefined;
        if (data?.error) return data.error;
        if (data?.errors?.length) return data.errors.map(e => e.errorMessage).join(', ');
    }
    return error instanceof Error ? error.message : fallback;
};

// ---------------------------------------------------------------------------
// Projects
// ---------------------------------------------------------------------------

export const getProjects = () =>
    http.get<ProjectDto[]>('/api/projects').then(r => r.data);

export const getProjectById = (id: string) =>
    http.get<ProjectDto>(`/api/projects/${id}`).then(r => r.data);

export const createProject = (body: CreateProjectRequest) =>
    http.post<ProjectDto>('/api/projects', body).then(r => r.data);

export const updateProject = (id: string, body: UpdateProjectRequest) =>
    http.put<ProjectDto>(`/api/projects/${id}`, body).then(r => r.data);

export const archiveProject = (id: string) =>
    http.post<ProjectDto>(`/api/projects/${id}/archive`).then(r => r.data);

export const restoreProject = (id: string) =>
    http.post<ProjectDto>(`/api/projects/${id}/restore`).then(r => r.data);

export const setProjectCriticalityScore = (projectId: string, criticalityScore: number) =>
    http.put<ProjectDto>(`/api/projects/${projectId}/criticality-score`, { criticalityScore }).then(r => r.data);

export const getProjectMembers = (projectId: string) =>
    http.get<AppUserDto[]>(`/api/projects/${projectId}/members`).then(r => r.data);

export const addProjectMember = (projectId: string, userId: string) =>
    http.post<AppUserDto>(`/api/projects/${projectId}/members`, { userId }).then(r => r.data);

export const removeProjectMember = (projectId: string, userId: string) =>
    http.delete(`/api/projects/${projectId}/members/${userId}`);

// ---------------------------------------------------------------------------
// Tasks
// ---------------------------------------------------------------------------

export const getTasksByProject = (projectId: string) =>
    http.get<ProjectTaskDto[]>(`/api/tasks/project/${projectId}`).then(r => r.data);

export const getTaskById = (id: string) =>
    http.get<ProjectTaskDto>(`/api/tasks/${id}`).then(r => r.data);

export const createTask = (body: CreateTaskRequest) =>
    http.post<ProjectTaskDto>('/api/tasks', body).then(r => r.data);

export const updateTask = (id: string, body: UpdateTaskRequest) =>
    http.put<ProjectTaskDto>(`/api/tasks/${id}`, body).then(r => r.data);

export const transitionTask = (id: string, newStatus: TaskStatus) =>
    http.post<ProjectTaskDto>(`/api/tasks/${id}/transition`, { newStatus }).then(r => r.data);

export const castVote = (taskId: string, voteValue: number) =>
    http.put<ProjectTaskDto>(`/api/tasks/${taskId}/votes`, { voteValue }).then(r => r.data);

// ---------------------------------------------------------------------------
// Phases
// ---------------------------------------------------------------------------

export const getPhasesByProject = (projectId: string) =>
    http.get<ProjectPhaseDto[]>(`/api/phases/project/${projectId}`).then(r => r.data);

export const createPhase = (body: CreatePhaseRequest) =>
    http.post<ProjectPhaseDto>('/api/phases', body).then(r => r.data);

export const updatePhase = (id: string, body: UpdatePhaseRequest) =>
    http.put<ProjectPhaseDto>(`/api/phases/${id}`, body).then(r => r.data);

export const deletePhase = (id: string) =>
    http.delete(`/api/phases/${id}`);

// ---------------------------------------------------------------------------
// Epics
// ---------------------------------------------------------------------------

export const getEpicsByProject = (projectId: string) =>
    http.get<EpicDto[]>(`/api/epics/project/${projectId}`).then(r => r.data);

export const createEpic = (body: CreateEpicRequest) =>
    http.post<EpicDto>('/api/epics', body).then(r => r.data);

export const updateEpic = (id: string, body: UpdateEpicRequest) =>
    http.put<EpicDto>(`/api/epics/${id}`, body).then(r => r.data);

export const deleteEpic = (id: string) =>
    http.delete(`/api/epics/${id}`);

// ---------------------------------------------------------------------------
// Users
// ---------------------------------------------------------------------------

export const getUsers = () =>
    http.get<AppUserDto[]>('/api/users').then(r => r.data);

export const getUserById = (id: string) =>
    http.get<AppUserDto>(`/api/users/${id}`).then(r => r.data);

export const createUser = (body: CreateUserRequest) =>
    http.post<AppUserDto>('/api/users', body).then(r => r.data);

export const setUserActive = (id: string, isActive: boolean) =>
    http.put<AppUserDto>(`/api/users/${id}/active`, { isActive }).then(r => r.data);

export const setUserAdmin = (id: string, isAdmin: boolean) =>
    http.put<AppUserDto>(`/api/users/${id}/admin`, { isAdmin }).then(r => r.data);

export const uploadUserAvatar = (id: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return http
        .post<AppUserDto>(`/api/users/${id}/avatar`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        })
        .then(r => r.data);
};

// ---------------------------------------------------------------------------
// Allowed Origins (CORS allow-list, admin-managed — PREP.1)
// ---------------------------------------------------------------------------

export const getAllowedOrigins = () =>
    http.get<AllowedOriginDto[]>('/api/allowed-origins').then(r => r.data);

export const addAllowedOrigin = (originUrl: string) =>
    http.post<AllowedOriginDto>('/api/allowed-origins', { originUrl }).then(r => r.data);

export const removeAllowedOrigin = (id: string) =>
    http.delete(`/api/allowed-origins/${id}`);
