import React from 'react';
import type { ITask, TaskStatus } from '../features/tasks/store/TaskInterfaces';
import './Css/TaskCard.css';

// Define the props interface for the TaskCard component.
interface TaskCardProps {
  task: ITask;
  isAdmin: boolean;
  // The ID of the currently logged-in user, used for assignment logic.
  currentUserId: string | undefined;

  // Callback functions for task actions.
  onEdit: (task: ITask) => void;
  onDelete: (task: ITask) => void;
  // Optional callback for self-assigning a task.
  onSelfAssign?: (task: ITask) => void;

  // Optional callback for when the card itself is clicked.
  onClick?: (taskId: string) => void;

  // NEW OPTIONAL PROP: Directly control if edit/delete buttons are shown.
  // If undefined, the component's internal logic (isAdmin || isAssignedToCurrentUser) applies.
  // If explicitly true/false, it overrides the internal logic.
  canShowActions?: boolean;
}

const TaskCard: React.FC<TaskCardProps> = ({
  task,
  isAdmin,
  currentUserId,
  onEdit,
  onDelete,
  onSelfAssign,
  onClick,
  canShowActions 
}) => {
  // Returns a CSS class name based on the task's status for styling.
  const getStatusClassName = (status: TaskStatus) => {
    switch (status) {
      case 'Not Started': return 'status-notstarted';
      case 'In Progress': return 'status-inprogress';
      case 'Completed': return 'status-completed';
      case 'On Hold': return 'status-onhold';
      case 'Blocked': return 'status-blocked';
      default: return '';
    }
  };

  // Formats the due date for display, or shows 'No Due Date'.
  const formattedDueDate = task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No Due Date';

  // Checks if the task is currently assigned to the logged-in user.
  const isAssignedToCurrentUser = currentUserId && task.userId === currentUserId;

  // Determine if the user has permission to edit or delete the task based on internal logic
  const internalCanEditOrDelete = isAdmin || isAssignedToCurrentUser;

  const finalCanShowActions = typeof canShowActions === 'boolean' ? canShowActions : internalCanEditOrDelete;


  return (
    <div
      className={`task-card ${getStatusClassName(task.status)}`}
      // Attach onClick handler if provided, stopping event propagation if present.
      onClick={onClick ? () => onClick(task.id) : undefined}
    >
      <div className="task-card-header">
        <h3 className="task-card-title">{task.title}</h3>
        {task.projectName && (
          <p className="task-card-project">Project: {task.projectName}</p>
        )}
      </div>
      <div className="task-card-body">
        <p className="task-card-description">{task.description || 'No description provided.'}</p>
        <p className="task-card-due-date">Due: {formattedDueDate}</p>
        <p className="task-card-status">Status: <span>{task.status}</span></p>
        {task.userId ? (
          <p className="task-card-assigned-to">
            Assigned to: {isAssignedToCurrentUser ? 'You' : (task.assignedUserEmail || 'Unknown User')}
          </p>
        ) : (
          <p className="task-card-assigned-to">Assigned to: Unassigned</p>
        )}
      </div>
      <div className="task-card-footer">
        <small className="task-card-dates">
          Created: {new Date(task.createdAt).toLocaleDateString()}
          {/* Display updated date if available and different from creation date. */}
          {task.updatedAt && task.createdAt !== task.updatedAt && (
            <span> | Updated: {new Date(task.updatedAt).toLocaleDateString()}</span>
          )}
        </small>
        <div className="task-card-actions">
          {/* Render Edit/Delete buttons only if finalCanShowActions is true. */}
          {finalCanShowActions && (
            <>
              <button
                className="button action-button edit-button"
                onClick={(e) => { e.stopPropagation(); onEdit(task); }}
              >
                Edit
              </button>
              <button
                className="button action-button delete-button"
                onClick={(e) => { e.stopPropagation(); onDelete(task); }}
              >
                Delete
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default TaskCard;