import { useState } from 'react';
import type { AppUserDto } from '@taskmanager/shared';
import { createTask } from '@taskmanager/shared';
import { Modal } from '../../components/Modal';
import { TaskForm } from './TaskForm';
import type { TaskFormValues } from './TaskForm';

interface CreateTaskModalProps {
    projectId: string;
    members: AppUserDto[];
    onClose: () => void;
    onCreated: () => void | Promise<void>;
}

export function CreateTaskModal({ projectId, members, onClose, onCreated }: CreateTaskModalProps) {
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (values: TaskFormValues) => {
        setSubmitting(true);
        setError(null);
        try {
            await createTask({
                projectId,
                title: values.title,
                priority: values.priority,
                dueDate: values.dueDate ?? undefined,
                assignedUserId: values.assigneeId ?? undefined,
            });
            await onCreated();
            onClose();
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to create task');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Modal isOpen onClose={onClose} title="New Task">
            <TaskForm
                members={members}
                onSubmit={values => void handleSubmit(values)}
                onCancel={onClose}
                submitting={submitting}
                error={error}
            />
        </Modal>
    );
}
