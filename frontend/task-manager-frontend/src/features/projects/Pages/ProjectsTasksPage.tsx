import React, { useEffect, useState, useMemo } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useParams, useNavigate } from 'react-router-dom';

import type { RootState, AppDispatch } from '../../../app/store';

import {
  fetchTasksByProjectId,
  createTask,
  updateTask,
  deleteTask,
  fetchAllProjectsForTasks,
} from '../../tasks/store/tasksSlice';

import type { ITask, ICreateTaskRequest, IUpdateTaskRequest } from '../../tasks/store/TaskInterfaces';

import { fetchAllUsers } from '../../user/store/usersSlice';

import TaskFormModal from '../../../components/TaskFormModal';
import TaskCard from '../../../components/TaskCard';
import DeleteConfirmationModal from '../../../components/DeleteConfirmationModal';

import './Css/ProjectTasksPage.css'; // Imports CSS for styling the page.

// ProjectTasksPage Functional Component: Displays and manages tasks for a specific project.
const ProjectTasksPage: React.FC = () => {
  // Extracts projectId from URL parameters.
  const { projectId } = useParams<{ projectId: string }>();
  // Hook for programmatic navigation.
  const navigate = useNavigate();
  // Initializes Redux dispatch for async actions.
  const dispatch: AppDispatch = useDispatch();

  // Selects tasks, all projects, and loading/error states from the Redux store.
  const { tasks, projects: allProjects, loading: tasksLoading, error: tasksError } = useSelector(
    (state: RootState) => state.tasks
  );
  // Selects all users and their loading/error states from the Redux store.
  const { users: allUsers, loading: usersLoading, error: usersError } = useSelector(
    (state: RootState) => state.users
  );
  // Selects the current authenticated user.
  const { user: currentUser } = useSelector((state: RootState) => state.auth) as { user: any };

  // Determines if the current user has 'Admin' role.
  const isAdmin = currentUser?.roles?.includes('Admin') || false;
  // Gets the ID of the current user.
  const currentUserId = currentUser?.id as string | undefined;

  // State to control the visibility of the task creation/edit modal.
  const [showTaskFormModal, setShowTaskFormModal] = useState(false);
  // State to hold the task being edited.
  const [taskToEdit, setTaskToEdit] = useState<ITask | null>(null);
  // State to control the visibility of the delete confirmation modal.
  const [showDeleteConfirmationModal, setShowDeleteConfirmationModal] = useState(false);
  // State to store the ID of the task to be deleted.
  const [taskToDeleteId, setTaskToDeleteId] = useState<string | null>(null);
  // State to store the name of the current project.
  const [currentProjectName, setCurrentProjectName] = useState<string | undefined>(undefined);

  // States for sorting tasks.
  const [sortBy, setSortBy] = useState<string>('dueDate'); // Default sort by due date.
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc'); // Default ascending order.

  // Effect hook to fetch tasks and related data when projectId changes.
  useEffect(() => {
    if (projectId) {
      dispatch(fetchTasksByProjectId(projectId)); // Fetches tasks specific to the project.
      dispatch(fetchAllProjectsForTasks()); // Fetches all projects (for task creation/editing dropdown).
      if (isAdmin) {
        dispatch(fetchAllUsers()); // Fetches all users if the current user is an admin.
      }
    } else {
      navigate('/projects'); // Redirects if no project ID is provided.
    }
  }, [projectId, dispatch, navigate, isAdmin]);

  // Effect hook to set the current project's name once projects are loaded.
  useEffect(() => {
    if (allProjects.length > 0 && projectId) {
      const foundProject = allProjects.find(p => p.id === projectId);
      if (foundProject) {
        setCurrentProjectName(foundProject.name);
      } else {
        setCurrentProjectName("Unknown Project"); // Fallback if project is not found.
      }
    }
  }, [allProjects, projectId]);

  // handleOpenCreateTaskModal: Prepares and opens the modal for new task creation.
  const handleOpenCreateTaskModal = () => {
    setTaskToEdit(null); // Ensures no existing task is pre-filled.
    setShowTaskFormModal(true);
  };

  // handleOpenEditTaskModal: Prepares task data for editing and opens the modal.
  // Checks user permissions before opening.
  const handleOpenEditTaskModal = (task: ITask) => {
    const canUserEditThisTask = isAdmin || (currentUserId && task.userId === currentUserId);
    if (!canUserEditThisTask) {
      alert("You do not have permission to edit this task.");
      return;
    }
    setTaskToEdit(task);
    setShowTaskFormModal(true);
  };

  // handleCloseTaskFormModal: Closes the task form modal and resets state.
  const handleCloseTaskFormModal = () => {
    setShowTaskFormModal(false);
    setTaskToEdit(null);
  };

  // handleSaveTask: Dispatches either create or update task action.
  // Formats date and description for backend compatibility.
  const handleSaveTask = async (taskData: ICreateTaskRequest | IUpdateTaskRequest) => {
    const formatDataForBackend = (data: typeof taskData) => {
      const formattedDueDate = data.dueDate
        ? new Date(data.dueDate).toISOString().substring(0, 10)
        : undefined;

      return {
        ...data,
        description: data.description === '' ? null : data.description, // Converts empty string to null for backend.
        dueDate: formattedDueDate,
      };
    };

    try {
      if (taskToEdit) {
        // Dispatches update action if editing an existing task.
        const dataToUpdate = formatDataForBackend({ ...taskData, projectId: taskData.projectId, id: taskToEdit.id });
        await dispatch(updateTask({ taskId: taskToEdit.id, taskData: dataToUpdate as IUpdateTaskRequest })).unwrap();
        alert('Task updated successfully!');
      } else {
        // Dispatches create action for a new task.
        const dataToCreate = formatDataForBackend({ ...taskData, projectId: taskData.projectId || projectId });
        await dispatch(createTask(dataToCreate as ICreateTaskRequest)).unwrap();
        alert('Task created successfully!');
      }
      dispatch(fetchTasksByProjectId(projectId!)); // Re-fetches tasks to update the list.
      handleCloseTaskFormModal();
    } catch (err: any) {
      alert(`Operation failed: ${err}`);
      console.error('Failed to save task:', err);
    }
  };

  // handleDeleteAttempt: Initiates the task deletion process.
  // Shows confirmation modal if user is admin, otherwise alerts no permission.
  const handleDeleteAttempt = (task: ITask) => {
    if (!isAdmin) {
      alert("You do not have permission to delete tasks.");
      return;
    }
    setTaskToDeleteId(task.id);
    setShowDeleteConfirmationModal(true);
  };

  // handleConfirmDelete: Executes the task deletion after confirmation.
  const handleConfirmDelete = async () => {
    if (taskToDeleteId) {
      try {
        await dispatch(deleteTask(taskToDeleteId)).unwrap();
        alert('Task deleted successfully!');
        dispatch(fetchTasksByProjectId(projectId!)); // Re-fetches tasks to update the list.
      } catch (err: any) {
        alert(`Failed to delete task: ${err}`);
        console.error('Failed to delete task:', err);
      } finally {
        // Resets modal and task ID regardless of success or failure.
        setShowDeleteConfirmationModal(false);
        setTaskToDeleteId(null);
      }
    }
  };

  // handleCancelDelete: Closes the delete confirmation modal without deleting.
  const handleCancelDelete = () => {
    setShowDeleteConfirmationModal(false);
    setTaskToDeleteId(null);
  };

  // handleSelfAssignTask: Allows a non-admin user to assign an unassigned task to themselves.
  const handleSelfAssignTask = async (task: ITask) => {
    if (!currentUserId) {
      alert("You must be logged in to assign tasks.");
      return;
    }
    if (isAdmin) {
      alert("Admins can assign tasks using the 'Edit Task' functionality.");
      return;
    }
    if (!task.userId || task.userId !== currentUserId) {
      if (window.confirm(`Are you sure you want to assign "${task.title}" to yourself?`)) {
        // Prepares update data for self-assignment.
        const updateData: IUpdateTaskRequest = {
          userId: currentUserId,
          title: task.title,
          description: task.description === '' ? null : task.description,
          dueDate: task.dueDate ? new Date(task.dueDate).toISOString().substring(0, 10) : undefined,
          status: task.status,
          projectId: task.projectId,
          id: '' // ID is passed via taskId parameter, not in taskData for updateTask.
        };
        try {
          await dispatch(updateTask({ taskId: task.id, taskData: updateData })).unwrap();
          alert(`Task "${task.title}" assigned to you successfully!`);
          dispatch(fetchTasksByProjectId(projectId!)); // Re-fetches tasks to reflect assignment.
        } catch (err: any) {
          alert(`Failed to assign task: ${err}`);
          console.error('Failed to self-assign task:', err);
        }
      }
    } else {
      alert("This task is already assigned to you.");
    }
  };

  // Filters tasks to show only those belonging to the current project.
  const projectSpecificTasks = tasks.filter(task => task.projectId === projectId);

  // useMemo for sorting tasks to optimize performance.
  const sortedTasks = useMemo(() => {
    const sortableTasks = [...projectSpecificTasks]; // Creates a mutable copy for sorting.

    // Custom order for task statuses, defining their display priority.
    const statusOrder: { [key: string]: number } = {
        'Not Started': 1,
        'In Progress': 2,
        'On Hold': 3,
        'Blocked': 4,
        'Completed': 5,
    };

    // Sorts tasks based on the selected sortBy field and sortOrder.
    sortableTasks.sort((a, b) => {
        let valA: any;
        let valB: any;

        switch (sortBy) {
            case 'dueDate':
                // Handles null/undefined due dates by placing them last in ascending order.
                valA = a.dueDate ? new Date(a.dueDate).getTime() : Infinity;
                valB = b.dueDate ? new Date(b.dueDate).getTime() : Infinity;
                break;
            case 'status':
                // Uses custom status order, placing unknown statuses at the end.
                valA = statusOrder[a.status] || Infinity;
                valB = statusOrder[b.status] || Infinity;
                break;
            case 'assignedUserEmail':
                // Sorts assigned user emails case-insensitively, unassigned first.
                valA = a.assignedUserEmail ? a.assignedUserEmail.toLowerCase() : '';
                valB = b.assignedUserEmail ? b.assignedUserEmail.toLowerCase() : '';
                break;
            case 'createdAt':
                // Sorts by creation date.
                valA = new Date(a.createdAt).getTime();
                valB = new Date(b.createdAt).getTime();
                break;
            default: // Default string comparison for 'title' or other string fields.
                valA = (a as any)[sortBy] ? (a as any)[sortBy].toLowerCase() : '';
                valB = (b as any)[sortBy] ? (b as any)[sortBy].toLowerCase() : '';
                break;
        }

        // Performs comparison based on data type.
        if (typeof valA === 'string' && typeof valB === 'string') {
            return sortOrder === 'asc' ? valA.localeCompare(valB) : valB.localeCompare(valA);
        } else {
            // Numeric or Date comparison for non-string types.
            return sortOrder === 'asc' ? (valA - valB) : (valB - valA);
        }
    });

    return sortableTasks;
  }, [projectSpecificTasks, sortBy, sortOrder]); // Dependencies for memoization.


  // Combines loading states for all relevant data.
  const isLoading = tasksLoading || usersLoading;
  // Combines error states for all relevant data.
  const hasError = tasksError || usersError;

  // Renders a message if no project ID is found.
  if (!projectId) {
    return <div className="project-tasks-container"><p className="project-tasks-message error-message">No project selected.</p></div>;
  }

  // JSX structure for the Project Tasks Page component.
  return (
    <div className="project-tasks-container">
      <h1 className="project-tasks-page-title">Tasks for Project: {currentProjectName || 'Loading...'}</h1>

      {currentUserId && (
        <div className="task-actions-header">
          <button onClick={handleOpenCreateTaskModal} className="button primary-button" disabled={isLoading}>
            Add New Task
          </button>
        </div>
      )}

      {/* Sorting Controls UI */}
      <div className="filter-sort-controls">
          <label htmlFor="sort-by">Sort By:</label>
          <select id="sort-by" value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
              <option value="dueDate">Due Date</option>
              <option value="title">Title</option>
              <option value="status">Status</option>
              <option value="assignedUserEmail">Assigned To</option>
              <option value="createdAt">Created Date</option>
          </select>

          <label htmlFor="sort-order">Order:</label>
          <select id="sort-order" value={sortOrder} onChange={(e) => setSortOrder(e.target.value as 'asc' | 'desc')}>
              <option value="asc">Ascending</option>
              <option value="desc">Descending</option>
          </select>
      </div>


      {isLoading && <p className="project-tasks-message">Loading tasks...</p>}
      {hasError && <p className="project-tasks-message error-message">Error: {hasError}</p>}

      {!isLoading && !hasError && sortedTasks.length === 0 ? ( // Displays message if no tasks are found.
        <p className="project-tasks-message no-tasks-found">
          No tasks found for this project. {currentUserId ? "Click 'Add New Task' to create one!" : ""}
        </p>
      ) : (
        <div className="task-list-grid">
          {sortedTasks.map(task => ( // Renders each sorted task using TaskCard component.
            <TaskCard
              key={task.id}
              task={task}
              isAdmin={isAdmin}
              currentUserId={currentUserId}
              onEdit={handleOpenEditTaskModal}
              onDelete={handleDeleteAttempt}
              onSelfAssign={handleSelfAssignTask}
            />
          ))}
        </div>
      )}

      {/* Task Form Modal for creating or editing tasks */}
      {showTaskFormModal && (
        <TaskFormModal
          isOpen={showTaskFormModal}
          onClose={handleCloseTaskFormModal}
          onSave={handleSaveTask}
          taskToEdit={taskToEdit}
          initialProjectId={projectId}
          projects={allProjects}
          users={allUsers}
          isAdmin={isAdmin}
          currentUserId={currentUserId || ''}
        />
      )}

      {/* Delete Confirmation Modal for task deletion */}
      {showDeleteConfirmationModal && (
        <DeleteConfirmationModal
          isOpen={showDeleteConfirmationModal}
          onClose={handleCancelDelete}
          onConfirm={handleConfirmDelete}
          message={`Are you sure you want to delete this task? This action cannot be undone.`}
        />
      )}
    </div>
  );
};

export default ProjectTasksPage;