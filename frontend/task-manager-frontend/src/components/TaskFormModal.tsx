import React, { useState, useEffect } from 'react';
import type { ITask, ICreateTaskRequest, IUpdateTaskRequest, TaskStatus, IProject } from '../features/tasks/store/TaskInterfaces';
import type { IUserBasic } from '../features/user/store/usersSlice';
import './Css/TaskFormModal.css';

interface TaskFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  // Callback to save task data (create or update).
  onSave: (taskData: ICreateTaskRequest | IUpdateTaskRequest) => Promise<void>;
  taskToEdit: ITask | null;
  initialProjectId?: string;
  projects: IProject[];
  // List of users; populated only if isAdmin is true.
  users: IUserBasic[];
  isAdmin: boolean;
  currentUserId: string;
}

const TaskFormModal: React.FC<TaskFormModalProps> = ({
  isOpen,
  onClose,
  onSave,
  taskToEdit,
  initialProjectId,
  projects,
  users,
  isAdmin,
  currentUserId,
}) => {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    status: 'Not Started' as TaskStatus,
    projectId: initialProjectId || '',
    // Holds the ID of the assigned user, or empty for unassigned.
    selectedUserId: '',
  });
  // State to manage form validation errors.
  const [formErrors, setFormErrors] = useState<{ [key: string]: string }>({});

  // Effect to initialize form data when the modal opens or taskToEdit changes.
  useEffect(() => {
    if (isOpen) {
      if (taskToEdit) {
        setFormData({
          title: taskToEdit.title,
          description: taskToEdit.description || '',
          dueDate: taskToEdit.dueDate ? taskToEdit.dueDate.substring(0, 10) : '',
          status: taskToEdit.status,
          projectId: taskToEdit.projectId,
          selectedUserId: taskToEdit.userId || '',
        });
      } else {
        setFormData({
          title: '',
          description: '',
          dueDate: '',
          status: 'Not Started' as TaskStatus,
          projectId: initialProjectId || '',
          // For new tasks, default assignment to current user if not an admin.
          selectedUserId: isAdmin ? '' : currentUserId,
        });
      }
      setFormErrors({}); // Clear any previous errors.
    }
  }, [taskToEdit, initialProjectId, isAdmin, currentUserId, isOpen]);

  // If the modal is not open, render nothing.
  if (!isOpen) return null;

  // Handles changes to form input fields and clears corresponding errors.
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    setFormErrors((prev) => ({ ...prev, [name]: '' }));
  };

  // Validates form inputs and sets error messages.
  const validateForm = () => {
    const errors: { [key: string]: string } = {};
    if (!formData.title.trim()) {
      errors.title = 'Title is required.';
    }
    if (!formData.dueDate) {
      errors.dueDate = 'Due Date is required.';
    }
    if (!formData.projectId) {
      errors.projectId = 'Project is required.';
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0; // Returns true if no errors.
  };

  // Handles form submission, validating data and preparing payload for onSave.
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) {
      return;
    }

    let userIdToSave: string | null;

    if (isAdmin) {
      // Admins can assign to any user or unassign.
      userIdToSave = formData.selectedUserId === '' ? null : formData.selectedUserId;
    } else {
      // Non-admins creating a task get it assigned to themselves.
      // Non-admins editing a task retain its current assignment.
      userIdToSave = taskToEdit ? (taskToEdit.userId ?? null) : currentUserId;
    }

    const baseTaskData = {
      title: formData.title,
      description: formData.description === '' ? null : formData.description,
      dueDate: formData.dueDate,
      projectId: formData.projectId,
      status: formData.status,
      userId: userIdToSave,
    };

    let dataToSend: ICreateTaskRequest | IUpdateTaskRequest;

    if (taskToEdit) {
      // For updates, include the task ID in the request payload.
      dataToSend = {
        id: taskToEdit.id,
        title: baseTaskData.title,
        description: baseTaskData.description,
        dueDate: baseTaskData.dueDate,
        projectId: baseTaskData.projectId,
        status: baseTaskData.status,
        userId: baseTaskData.userId,
      } as IUpdateTaskRequest;
    } else {
      // For creation, no ID is needed in the request payload.
      dataToSend = {
        title: baseTaskData.title,
        description: baseTaskData.description,
        dueDate: baseTaskData.dueDate,
        projectId: baseTaskData.projectId,
        status: baseTaskData.status,
        userId: baseTaskData.userId,
      } as ICreateTaskRequest;
    }

    await onSave(dataToSend);
  };

  const taskStatuses: TaskStatus[] = ['Not Started', 'In Progress', 'Completed', 'On Hold', 'Blocked'];

  // Helper function to get the assigned user's name for display.
  const getAssignedUserNameForDisplay = (id: string | null | undefined): string => {
    if (!id) return 'Unassigned';

    if (id === currentUserId) {
      return 'You';
    }

    // Only attempt to find user by name if admin has fetched all users.
    if (isAdmin && users.length > 0) {
      const foundUser = users.find(u => u.id === id);
      if (foundUser) {
        return foundUser.name || foundUser.email || 'Assigned User';
      }
    }
    return 'Assigned';
  };

  // Determines if a non-admin user can edit the current task.
  const canNonAdminEditOwnTask = !isAdmin && taskToEdit && taskToEdit.userId === currentUserId;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>{taskToEdit ? 'Edit Task' : 'Create New Task'}</h2>
        <form onSubmit={handleSubmit}>
          {/* Title input field */}
          <div className="form-group">
            <label htmlFor="title">Title:</label>
            <input
              type="text"
              id="title"
              name="title"
              value={formData.title}
              onChange={handleChange}
              className={formErrors.title ? 'input-error' : ''}
              // Disable field if not admin, and it's an existing task not assigned to current user.
              disabled={!isAdmin && !!taskToEdit && !canNonAdminEditOwnTask}
            />
            {formErrors.title && <span className="error-message-inline">{formErrors.title}</span>}
          </div>

          {/* Description textarea */}
          <div className="form-group">
            <label htmlFor="description">Description (Optional):</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={4}
              disabled={!isAdmin && !!taskToEdit && !canNonAdminEditOwnTask}
            />
          </div>

          {/* Due Date input field */}
          <div className="form-group">
            <label htmlFor="dueDate">Due Date:</label>
            <input
              type="date"
              id="dueDate"
              name="dueDate"
              value={formData.dueDate}
              onChange={handleChange}
              className={formErrors.dueDate ? 'input-error' : ''}
              disabled={!isAdmin && !!taskToEdit && !canNonAdminEditOwnTask}
            />
            {formErrors.dueDate && <span className="error-message-inline">{formErrors.dueDate}</span>}
          </div>

          {/* Status dropdown */}
          <div className="form-group">
            <label htmlFor="status">Status:</label>
            <select
              id="status"
              name="status"
              value={formData.status}
              onChange={handleChange}
              disabled={!isAdmin && !!taskToEdit && !canNonAdminEditOwnTask}
            >
              {taskStatuses.map(status => (
                <option key={status} value={status}>{status}</option>
              ))}
            </select>
          </div>

          {/* Project dropdown */}
          <div className="form-group">
            <label htmlFor="projectId">Project:</label>
            <select
              id="projectId"
              name="projectId"
              value={formData.projectId}
              onChange={handleChange}
              className={formErrors.projectId ? 'input-error' : ''}
              disabled={!isAdmin && !!taskToEdit && !canNonAdminEditOwnTask}
            >
              <option value="">Select a Project</option>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
            {formErrors.projectId && <span className="error-message-inline">{formErrors.projectId}</span>}
          </div>

          {/* Assign To field - conditionally rendered based on isAdmin */}
          {isAdmin ? (
            <div className="form-group">
              <label htmlFor="selectedUserId">Assign To: </label>
              <select
                id="selectedUserId"
                name="selectedUserId"
                value={formData.selectedUserId}
                onChange={handleChange}
              >
                <option value="">Unassigned</option>
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.name || user.email} ({user.roles.join(', ')})
                  </option>
                ))}
              </select>
            </div>
          ) : (
            <div className="form-group">
              <label>Assigned To:</label>
              <input
                type="text"
                value={taskToEdit && taskToEdit.userId === currentUserId ? 'You' : getAssignedUserNameForDisplay(taskToEdit?.userId)}
                readOnly
                disabled // This field is always disabled for non-admins as they cannot change assignment.
              />
              {!taskToEdit && <p className="form-hint">New tasks are by default assigned to you</p>}
            </div>
          )}

          <div className="modal-actions">
            {/* Submit button is visible if creating a new task, or if it's an admin,
                or if it's an existing task assigned to the current user. */}
            {(!taskToEdit || (isAdmin || (taskToEdit && taskToEdit.userId === currentUserId))) && (
              <button type="submit" className="button primary-button">
                {taskToEdit ? 'Save Changes' : 'Create Task'}
              </button>
            )}
            <button type="button" className="button secondary-button" onClick={onClose}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskFormModal;