import { useEffect, useState } from 'react';
import { ListChecks, Plus, Users, Layers } from 'lucide-react';
import type { ProjectTaskDto } from '@taskmanager/shared';
import { useProject } from '../../contexts/ProjectContext';
import { useProjectDetail } from '../../hooks/useProjectDetail';
import { useTasks } from '../../hooks/useTasks';
import { usePhases } from '../../hooks/usePhases';
import { useEpics } from '../../hooks/useEpics';
import { useUsers } from '../../hooks/useUsers';
import { useProjectMembers } from '../../hooks/useProjectMembers';
import { KanbanBoard } from '../../features/board/KanbanBoard';
import { ListView } from '../../features/board/ListView';
import { TodoList } from '../../features/board/TodoList';
import { TaskDetailPanel } from '../../features/tasks/TaskDetailPanel';
import { CreateTaskModal } from '../../features/tasks/CreateTaskModal';
import { MembersPanel } from '../../features/project/MembersPanel';
import { EpicsPanel } from '../../features/project/EpicsPanel';
import { ViewToggle, type ViewMode } from './ViewToggle';
import { useBoardScreenStyles } from './BoardScreen.styles';

const STORAGE_KEY = 'tm:viewMode';
const DEFAULT_ACCENT = '#991b1b'; // primary-800, used when a project has no stored color

function loadViewMode(): ViewMode {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === 'list' ? 'list' : 'kanban';
}

export function BoardScreen() {
    const { selectedProjectId } = useProject();
    const [viewMode, setViewMode] = useState<ViewMode>(loadViewMode);
    const [selectedTask, setSelectedTask] = useState<ProjectTaskDto | null>(null);
    const [showCreateTask, setShowCreateTask] = useState(false);
    const [showMembersPanel, setShowMembersPanel] = useState(false);
    const [showEpicsPanel, setShowEpicsPanel] = useState(false);
    const styles = useBoardScreenStyles();

    const { data: project } = useProjectDetail(selectedProjectId);
    const { data: tasks,  isLoading: loadingTasks, error: tasksError, refetch: refetchTasks } = useTasks(selectedProjectId);
    const { data: phases, isLoading: loadingPhases } = usePhases(selectedProjectId);
    const { data: epics,  isLoading: loadingEpics, refetch: refetchEpics } = useEpics(selectedProjectId);
    const { data: users,  isLoading: loadingUsers  } = useUsers();
    const { data: members, refetch: refetchMembers } = useProjectMembers(selectedProjectId);

    const loading = loadingTasks || loadingPhases || loadingEpics || loadingUsers;
    const accent = project?.colorHex ? `#${project.colorHex}` : DEFAULT_ACCENT;
    const isDailyTask = project?.scope === 'DailyTask';

    useEffect(() => {
        localStorage.setItem(STORAGE_KEY, viewMode);
    }, [viewMode]);

    return (
        <div className={styles.root}>
            <div className={styles.header}>
                <div
                    className={styles.titleWrap}
                    style={{
                        borderColor: accent,
                        boxShadow: `4px 4px 16px -1px ${accent}80`,
                        clipPath: 'inset(0 -24px -24px 0)',
                    }}
                >
                    <h2 className={styles.projectName}>{project?.name ?? ' '}</h2>
                </div>
                <div className={styles.headerActions}>
                    {selectedProjectId && (
                        <button type="button" onClick={() => setShowCreateTask(true)} className={styles.newTaskButton} aria-label="New task" title="New task">
                            <Plus className={styles.newTaskButtonIcon} />
                            <span className={styles.buttonLabel}>New Task</span>
                        </button>
                    )}
                    {selectedProjectId && (
                        <button type="button" onClick={() => setShowMembersPanel(true)} className={styles.membersButton} aria-label="Members" title="Members">
                            <Users className={styles.membersButtonIcon} />
                            <span className={styles.buttonLabel}>Members</span>
                        </button>
                    )}
                    {selectedProjectId && project?.scope === 'Project' && (
                        <button type="button" onClick={() => setShowEpicsPanel(true)} className={styles.membersButton} aria-label="Epics" title="Epics">
                            <Layers className={styles.membersButtonIcon} />
                            <span className={styles.buttonLabel}>Epics</span>
                        </button>
                    )}
                    {!isDailyTask && (
                        <ViewToggle mode={viewMode} onChange={setViewMode} />
                    )}
                </div>
            </div>

            {loading && (
                <p className={styles.loadingText}>Loading…</p>
            )}

            {!loading && tasksError && (
                <p className={styles.errorText}>Failed to load — check the API is running</p>
            )}

            {!loading && !tasksError && tasks.length === 0 && (
                <div className={styles.emptyState}>
                    <ListChecks className={styles.emptyIcon} />
                    <p className={styles.emptyTitle}>No tasks yet</p>
                    <p className={styles.emptyHint}>Create your first task to get started</p>
                </div>
            )}

            {!loading && tasks.length > 0 && isDailyTask && (
                <TodoList tasks={tasks} onTaskClick={setSelectedTask} />
            )}
            {!loading && tasks.length > 0 && !isDailyTask && viewMode === 'kanban' && (
                <KanbanBoard tasks={tasks} phases={phases} epics={epics} users={users} onTaskClick={setSelectedTask} />
            )}
            {!loading && tasks.length > 0 && !isDailyTask && viewMode === 'list' && (
                <ListView tasks={tasks} phases={phases} epics={epics} users={users} onTaskClick={setSelectedTask} />
            )}

            {selectedTask && (
                <TaskDetailPanel
                    task={selectedTask}
                    users={users}
                    members={members}
                    phases={phases}
                    epics={epics}
                    projectScope={project?.scope ?? 'Project'}
                    onClose={() => setSelectedTask(null)}
                    refetch={refetchTasks}
                />
            )}

            {showCreateTask && selectedProjectId && (
                <CreateTaskModal
                    projectId={selectedProjectId}
                    members={members}
                    onClose={() => setShowCreateTask(false)}
                    onCreated={refetchTasks}
                />
            )}

            {showMembersPanel && selectedProjectId && (
                <MembersPanel
                    projectId={selectedProjectId}
                    users={users}
                    usersLoading={loadingUsers}
                    onClose={() => setShowMembersPanel(false)}
                    onMembershipChanged={refetchMembers}
                />
            )}

            {showEpicsPanel && selectedProjectId && (
                <EpicsPanel
                    projectId={selectedProjectId}
                    projectColorHex={project?.colorHex}
                    onClose={() => setShowEpicsPanel(false)}
                    onEpicsChanged={refetchEpics}
                />
            )}
        </div>
    );
}
