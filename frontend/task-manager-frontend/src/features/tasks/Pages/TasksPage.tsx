import React, { useEffect, useState, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate, useParams } from 'react-router-dom';

import type { RootState, AppDispatch } from '../../../app/store';

import {
  fetchMyTasks,
  fetchTasksByUserId, // Imported to fetch tasks for a specific user (admin view).
  deleteTask,
  fetchAllProjectsForTasks,
  createTask,
  updateTask,
} from '../store/tasksSlice';

import { fetchAllUsers, type IUserBasic } from '../../user/store/usersSlice'; // Imports user types and fetch action.

import type { ITask, TaskStatus, ICreateTaskRequest, IUpdateTaskRequest } from '../store/TaskInterfaces';

import './Css/TasksPage.css'; // Imports CSS for styling.
import TaskCard from '../../../components/TaskCard'; // Component for displaying individual tasks.
import DeleteConfirmationModal from '../../../components/DeleteConfirmationModal'; // Modal for confirming deletions.
import TaskFormModal from '../../../components/TaskFormModal'; // Modal for creating/editing tasks.

// Defines options for sorting tasks.
const SORT_BY_OPTIONS = [
  { value: 'dueDate', label: 'Due Date' },
  { value: 'status', label: 'Status' },
  { value: 'project', label: 'Project' },
  { value: 'createdAt', label: 'Created At' },
  { value: 'title', label: 'Title' },
];

// Defines standard task status options.
const TASK_STATUS_OPTIONS: TaskStatus[] = ['Not Started', 'In Progress', 'Completed', 'On Hold', 'Blocked'];

// MyTasksPage Functional Component: Displays and manages tasks for the current user or a specific user (admin view).
const MyTasksPage: React.FC = () => {
  const dispatch: AppDispatch = useDispatch();
  const navigate = useNavigate();
  const { userId } = useParams<{ userId: string }>(); // Extracts userId from URL, if present (for admin view).

  // Selects tasks, loading/error states, and projects from the Redux store.
  const { tasks, loading, error, projects } = useSelector((state: RootState) => state.tasks);
  // Selects all users from the Redux store.
  const { users } = useSelector((state: RootState) => state.users);
  // Selects the current authenticated user.
  const { user: currentUser } = useSelector((state: RootState) => state.auth) as { user: any };

  // Determines if the current user is an administrator.
  const isAdmin = currentUser?.roles?.includes('Admin') || false;
  // Gets the ID of the current logged-in user.
  const currentUserId = currentUser?.id as string | undefined;

  // State to store the name of the user whose tasks are currently being viewed (if an admin is viewing another user's tasks).
  const [viewingUserName, setViewingUserName] = useState<string | undefined>(undefined);

  // States for sorting and filtering tasks.
  const [sortBy, setSortBy] = useState('dueDate');
  const [sortOrder, setSortByOrder] = useState('asc'); // Sort order ('asc' or 'desc').
  const [filterStatus, setFilterStatus] = useState(''); // Filter tasks by status.

  // States for delete confirmation modal.
  const [showDeleteConfirmModal, setShowDeleteConfirmModal] = useState(false);
  const [taskToDelete, setTaskToDelete] = useState<ITask | null>(null);

  // States for task form modal (create/edit).
  const [showTaskFormModal, setShowTaskFormModal] = useState(false);
  const [taskToEdit, setTaskToEdit] = useState<ITask | null>(null);

  // useCallback hook to memoize refetchTasks for performance, preventing unnecessary re-creations.
  const refetchTasks = useCallback(() => {
    if (userId) {
      // If a userId is present in the URL, an admin is viewing a specific user's tasks.
      dispatch(fetchTasksByUserId({ userId, sortBy, sortOrder, status: filterStatus }));
    } else {
      // Otherwise, the current user is viewing their own tasks.
      dispatch(fetchMyTasks({ sortBy, sortOrder, status: filterStatus }));
    }
  }, [dispatch, userId, sortBy, sortOrder, filterStatus]); // Dependencies for useCallback.

  // useEffect hook to fetch initial data when the component mounts or dependencies change.
  useEffect(() => {
    refetchTasks(); // Fetches tasks based on the current context (own tasks or specific user's tasks).
    dispatch(fetchAllProjectsForTasks()); // Fetches all projects for the task form's project dropdown.

    // Fetches all users if the current user is an admin OR if a specific user's tasks are being viewed (implies admin access).
    if (isAdmin || userId) {
      dispatch(fetchAllUsers());
    }
  }, [dispatch, refetchTasks, isAdmin, userId]); // Dependencies for useEffect.

  // useEffect hook to set the page title based on the user whose tasks are being viewed.
  useEffect(() => {
    if (userId && users.length > 0) {
      // Finds the target user from the fetched users list.
      const targetUser = users.find((user: IUserBasic) => user.id === userId);
      // Sets the viewing user's name, falling back to email or 'Unknown User'.
      setViewingUserName(targetUser?.name || targetUser?.email || 'Unknown User');
    } else if (!userId) {
      setViewingUserName(undefined); // Resets name when viewing own tasks.
    }
  }, [userId, users]); // Dependencies for useEffect.

  // handleSortChange: Updates the sortBy state based on user selection.
  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSortBy(e.target.value);
  };

  // handleSortOrderToggle: Toggles the sort order between ascending and descending.
  const handleSortOrderToggle = () => {
    setSortByOrder(prevOrder => (prevOrder === 'asc' ? 'desc' : 'asc'));
  };

  // handleFilterStatusChange: Updates the filterStatus state based on user selection.
  const handleFilterStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilterStatus(e.target.value);
  };

  // handleCreateTaskClick: Opens the task form modal for creating a new task.
  const handleCreateTaskClick = () => {
    setTaskToEdit(null); // Clears any pre-filled task data.
    setShowTaskFormModal(true);
  };

  // handleEditTaskClick: Opens the task form modal with an existing task for editing.
  const handleEditTaskClick = (task: ITask) => {
    setTaskToEdit(task);
    setShowTaskFormModal(true);
  };

  // handleDeleteClick: Sets the task to be deleted and opens the delete confirmation modal.
  const handleDeleteClick = (task: ITask) => {
    setTaskToDelete(task);
    setShowDeleteConfirmModal(true);
  };

  // confirmDeleteTask: Dispatches the deleteTask action after user confirmation.
  const confirmDeleteTask = async () => {
    if (taskToDelete) {
      const resultAction = await dispatch(deleteTask(taskToDelete.id));
      if (deleteTask.fulfilled.match(resultAction)) {
        alert(`Task "${taskToDelete.title}" deleted successfully.`);
        refetchTasks(); // Re-fetches tasks to update the list after deletion.
      } else {
        alert(`Failed to delete task: ${resultAction.payload || 'Unknown error'}`);
      }
      setShowDeleteConfirmModal(false);
      setTaskToDelete(null);
    }
  };

  // handleSaveTask: Dispatches either createTask or updateTask based on context.
  const handleSaveTask = async (taskData: ICreateTaskRequest | IUpdateTaskRequest) => {
    let resultAction;
    if (taskToEdit) {
      // If editing an existing task, dispatch updateTask.
      resultAction = await dispatch(updateTask({ taskId: taskToEdit.id, taskData: taskData as IUpdateTaskRequest }));
      if (updateTask.fulfilled.match(resultAction)) {
        alert(`Task "${taskData.title}" updated successfully.`);
        refetchTasks(); // Re-fetches tasks after successful update.
      } else {
        alert(`Failed to update task: ${resultAction.payload || 'Unknown error'}`);
      }
    } else {
      // If creating a new task, determine the assigned user ID.
      let taskDataToSend: ICreateTaskRequest = { ...taskData as ICreateTaskRequest };
      if (userId && isAdmin && !taskDataToSend.assignedTo) {
        // If admin is creating a task on another user's page, default assignment to that user.
        taskDataToSend.assignedTo = userId;
      } else if (!taskDataToSend.assignedTo && currentUserId) {
        // If current user creates a task for themselves, default assignment to current user.
        taskDataToSend.assignedTo = currentUserId;
      }
      resultAction = await dispatch(createTask(taskDataToSend));
      if (createTask.fulfilled.match(resultAction)) {
        alert(`Task "${taskData.title}" created successfully.`);
        refetchTasks(); // Re-fetches tasks after successful creation.
      } else {
        alert(`Failed to create task: ${resultAction.payload || 'Unknown error'}`);
      }
    }
    setShowTaskFormModal(false);
    setTaskToEdit(null);
  };

  // Dynamic page title: Displays "Tasks for [UserName]" or "Your Tasks".
  const pageTitle = userId && viewingUserName ? `Tasks for ${viewingUserName}` : 'Your Tasks';

  // canCreateTask: Determines if the "Add New Task" button should be enabled.
  // Admins can create tasks for any user; regular users can only create for themselves (when userId is not present).
  const canCreateTask = isAdmin || !userId;

  // taskFormDefaultAssignedTo: Provides a default user ID for the task assignment dropdown in the modal.
  // If an admin is on a specific user's page, it defaults to that user; otherwise, to the current logged-in user.
  const taskFormDefaultAssignedTo = userId ? userId : currentUserId;

  // Renders loading message while data is being fetched.
  if (loading) {
    return <div className="tasks-message">Loading tasks...</div>;
  }

  // Renders error message if fetching tasks failed.
  if (error) {
    return <div className="tasks-message error-message">Error: {error}</div>;
  }

  // JSX structure for the MyTasksPage component.
  return (
    <div className="tasks-page-container">
      <h1 className="tasks-page-title">{pageTitle}</h1>

      <div className="tasks-content-layout">
        <div className="tasks-control-panel">
          {canCreateTask && (
            <button className="button primary-button add-task-button" onClick={handleCreateTaskClick}>
              Add New Task
            </button>
          )}

          <div className="sort-filter-group">
            <div className="control-item">
              <label htmlFor="sortBy">Sort By:</label>
              <select id="sortBy" value={sortBy} onChange={handleSortChange}>
                {SORT_BY_OPTIONS.map(option => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </div>

            <button className="button secondary-button" onClick={handleSortOrderToggle}>
              Order: {sortOrder === 'asc' ? 'ASC ↑' : 'DESC ↓'}
            </button>

            <div className="control-item">
              <label htmlFor="filterStatus">Filter Status:</label>
              <select id="filterStatus" value={filterStatus} onChange={handleFilterStatusChange}>
                <option value="">All Statuses</option>
                {TASK_STATUS_OPTIONS.map(status => (
                  <option key={status} value={status}>{status}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        <div className="tasks-display-panel">
          {tasks.length === 0 ? (
            <div className="tasks-message no-tasks-found">
              <p>{userId && viewingUserName ? `No tasks found for ${viewingUserName}.` : 'You currently have no tasks assigned to you.'}</p>
              {canCreateTask && <p>Click "Add New Task" to get started!</p>}
            </div>
          ) : (
            <div className="task-list-grid">
              {tasks.map(task => (
                <TaskCard
                  key={task.id}
                  task={task}
                  onEdit={handleEditTaskClick}
                  onDelete={handleDeleteClick}
                  isAdmin={isAdmin}
                  currentUserId={currentUserId ?? ''} // Provides currentUserId for permission checks within TaskCard.
                />
              ))}
            </div>
          )}
        </div>
      </div>

      <DeleteConfirmationModal
        isOpen={showDeleteConfirmModal}
        onClose={() => setShowDeleteConfirmModal(false)}
        onConfirm={confirmDeleteTask}
        message={`Are you sure you want to delete task: "${taskToDelete?.title}"?`}
      />

      {showTaskFormModal && (
        <TaskFormModal
          isOpen={showTaskFormModal}
          onClose={() => {
            setShowTaskFormModal(false);
            setTaskToEdit(null); // Resets taskToEdit when modal closes.
          }}
          onSave={handleSaveTask}
          taskToEdit={taskToEdit}
          projects={projects} // Provides all projects for project assignment dropdown.
          users={users} // Provides all users for task assignment dropdown.
          isAdmin={isAdmin}
          currentUserId={currentUserId ?? ''}
          defaultAssignedToUserId={taskFormDefaultAssignedTo} // Sets default user for assignment.
        />
      )}
    </div>
  );
};

export default MyTasksPage;