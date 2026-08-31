import type { ProjectDto } from '@taskmanager/shared';
import { useProjectCardStyles } from './ProjectCard.styles';

const DEFAULT_ACCENT = '#991b1b'; // primary-800, used when a project has no stored color

interface ProjectCardProps {
    project: ProjectDto;
    onClick: () => void;
}

export function ProjectCard({ project, onClick }: ProjectCardProps) {
    const styles = useProjectCardStyles();
    const accent = project.colorHex ? `#${project.colorHex}` : DEFAULT_ACCENT;
    const taskCount = project.tasks?.length ?? 0;

    return (
        <button type="button" onClick={onClick} className={styles.card}>
            <span className={styles.accentBar} style={{ backgroundColor: accent }} />
            <div className={styles.body}>
                <p className={styles.name}>{project.name}</p>
                {project.description && (
                    <p className={styles.description}>{project.description}</p>
                )}
                <p className={styles.meta}>
                    {project.scope} · {taskCount} task{taskCount !== 1 ? 's' : ''}
                </p>
            </div>
        </button>
    );
}
