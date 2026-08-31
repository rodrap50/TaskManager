import { createContext, useContext, useState, type ReactNode } from 'react';

interface ProjectContextValue {
    selectedProjectId: string | null;
    selectProject: (id: string | null) => void;
}

const ProjectContext = createContext<ProjectContextValue | null>(null);

export function ProjectProvider({ children }: { children: ReactNode }) {
    const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);

    return (
        <ProjectContext.Provider value={{ selectedProjectId, selectProject: setSelectedProjectId }}>
            {children}
        </ProjectContext.Provider>
    );
}

export function useProject() {
    const ctx = useContext(ProjectContext);
    if (!ctx) throw new Error('useProject must be used within ProjectProvider');
    return ctx;
}
