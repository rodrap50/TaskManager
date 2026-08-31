import { useState } from 'react';
import { FolderOpen, Plus } from 'lucide-react';
import { useProjects } from '../../hooks/useProjects';
import { useProject } from '../../contexts/ProjectContext';
import { ProjectCard } from './ProjectCard';
import { CreateProjectModal } from './CreateProjectModal';
import { useProjectSelectorScreenStyles } from './ProjectSelectorScreen.styles';

export function ProjectSelectorScreen() {
    const { data: projects, isLoading, error } = useProjects();
    const { selectProject } = useProject();
    const [showModal, setShowModal] = useState(false);
    const styles = useProjectSelectorScreenStyles();

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>Projects</h2>
                <button type="button" onClick={() => setShowModal(true)} className={styles.newButton}>
                    <Plus className={styles.newButtonIcon} />
                    New
                </button>
            </div>

            {isLoading && (
                <p className={styles.loadingText}>Loading…</p>
            )}

            {!isLoading && error && (
                <p className={styles.errorText}>Failed to load — check the API is running</p>
            )}

            {!isLoading && !error && projects.length === 0 && (
                <div className={styles.emptyState}>
                    <FolderOpen className={styles.emptyIcon} />
                    <p className={styles.emptyTitle}>No projects yet</p>
                    <p className={styles.emptyHint}>Tap "New" to create your first project</p>
                </div>
            )}

            {[...projects]
                .sort((a, b) => b.criticalityScore - a.criticalityScore)
                .map(p => (
                    <ProjectCard key={p.id} project={p} onClick={() => selectProject(p.id)} />
                ))}

            {showModal && <CreateProjectModal onClose={() => setShowModal(false)} />}
        </div>
    );
}
