import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import api from '../../../services/axios'; 
import type { RootState } from '../../../app/store'; 
import type { ITask, ICreateTaskRequest, IUpdateTaskRequest, IProject } from './TaskInterfaces'; 

// TasksState Interface: Defines the structure of the tasks slice state.
interface TasksState {
  tasks: ITask[]; // List of tasks fetched for display.
  projects: IProject[]; // List of projects, used for dropdowns in task forms.
  loading: boolean; // Indicates if an async operation is in progress.
  error: string | null; // Stores any error messages.
}

// Initial State: Sets the default values for the tasks slice.
const initialState: TasksState = {
  tasks: [],
  projects: [],
  loading: false,
  error: null,
};

// fetchTasksByProjectId Async Thunk: Fetches tasks associated with a specific project ID.
// Mapped to: GET /api/projects/{projectId}/tasks
export const fetchTasksByProjectId = createAsyncThunk<
  ITask[], // Type of the fulfilled payload.
  string, // Type of the argument (projectId).
  { state: RootState; rejectValue: string } // ThunkAPI types for state access and error handling.
>(
  'tasks/fetchTasksByProjectId',
  async (projectId, { rejectWithValue }) => {
    try {
      const response = await api.get<ITask[]>(`/api/projects/${projectId}/tasks`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch tasks for project.');
    }
  }
);

// fetchMyTasks Async Thunk: Fetches all tasks assigned to the currently authenticated user.
// Allows optional filtering and sorting parameters.
// Mapped to: GET /api/tasks/my-tasks
export const fetchMyTasks = createAsyncThunk<
  ITask[], // Return type.
  { status?: string; sortBy?: string; sortOrder?: string; projectId?: string } | void, // Optional arguments for filtering/sorting.
  { state: RootState; rejectValue: string }
>(
  'tasks/fetchMyTasks',
  async (params = {}, { rejectWithValue }) => { // Defaults params to an empty object if none provided.
    try {
      const response = await api.get<ITask[]>('/api/tasks/my-tasks', { params });
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch all user tasks.');
    }
  }
);

// fetchTasksByUserId Async Thunk: Fetches all tasks for a specific user ID. (Admin view)
// Mapped to: GET /api/tasks/users/{userId}
export const fetchTasksByUserId = createAsyncThunk<
  ITask[], // Return type.
  { userId: string; status?: string; sortBy?: string; sortOrder?: string }, // Arguments including the target userId.
  { state: RootState; rejectValue: string }
>(
  'tasks/fetchTasksByUserId',
  async ({ userId, ...params }, { rejectWithValue }) => { // Destructures userId and collects other parameters.
    try {
      const response = await api.get<ITask[]>(`/api/tasks/users/${userId}`, { params });
      return response.data;
    } catch (error: any) {
      console.error(`Error fetching tasks for user ${userId}:`, error.response?.data || error.message);
      return rejectWithValue(error.response?.data?.message || `Failed to fetch tasks for user ${userId}.`);
    }
  }
);


// createTask Async Thunk: Sends a request to create a new task.
// Mapped to: POST /api/tasks
export const createTask = createAsyncThunk<
  ITask, // Return type (the created task).
  ICreateTaskRequest, // Payload type for the request.
  { state: RootState; rejectValue: string }
>(
  'tasks/createTask',
  async (taskData, { rejectWithValue }) => {
    try {
      const response = await api.post<ITask>(`/api/tasks`, taskData);
      return response.data; // Returns the created task.
    } catch (error: any) {
      // Handles validation errors returned from the backend.
      if (error.response?.data?.errors) {
        const validationErrors = error.response.data.errors;
        let errorMessage = 'Validation failed:';
        for (const key in validationErrors) {
          errorMessage += `\n${key}: ${validationErrors[key].join(', ')}`;
        }
        return rejectWithValue(errorMessage);
      }
      return rejectWithValue(error.response?.data?.message || 'Failed to create task.');
    }
  }
);

// updateTask Async Thunk: Sends a request to update an existing task.
// Mapped to: PUT /api/tasks/{taskId}
export const updateTask = createAsyncThunk<
  ITask, // Return type of the payload (the updated task).
  { taskId: string; taskData: IUpdateTaskRequest }, // Payload containing task ID and update data.
  { state: RootState; rejectValue: string }
>(
  'tasks/updateTask',
  async ({ taskId, taskData }, { rejectWithValue }) => {
    try {
      const response = await api.put<ITask>(`/api/tasks/${taskId}`, taskData);
      return response.data; // Returns the updated task.
    } catch (error: any) {
      // Handles validation errors.
      if (error.response?.data?.errors) {
        const validationErrors = error.response.data.errors;
        let errorMessage = 'Validation failed:';
        for (const key in validationErrors) {
          errorMessage += `\n${key}: ${validationErrors[key].join(', ')}`;
        }
        return rejectWithValue(errorMessage);
      }
      return rejectWithValue(error.response?.data?.message || 'Failed to update task.');
    }
  }
);

// deleteTask Async Thunk: Sends a request to delete a task.
// Mapped to: DELETE /api/tasks/{taskId}
export const deleteTask = createAsyncThunk<
  string, // Return type (the ID of the deleted task).
  string, // Argument type: taskId.
  { state: RootState; rejectValue: string }
>(
  'tasks/deleteTask',
  async (taskId, { rejectWithValue }) => {
    try {
      await api.delete(`/api/tasks/${taskId}`);
      return taskId; // Returns the ID of the deleted task.
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete task.');
    }
  }
);

// fetchAllProjectsForTasks Async Thunk: Fetches all projects, specifically for use in task creation/editing dropdowns.
// Mapped to: GET /api/projects/for-selection
export const fetchAllProjectsForTasks = createAsyncThunk<
  IProject[], // Return type.
  void, // Argument type (none).
  { state: RootState; rejectValue: string }
>(
  'tasks/fetchAllProjectsForTasks',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IProject[]>('/api/projects/for-selection');
      return response.data;
    } catch (error: any) {
      console.error('Failed to fetch projects for selection:', error.response?.data || error.message);
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch projects for task management.');
    }
  }
);


// tasksSlice: Defines the Redux slice for managing task-related state.
const tasksSlice = createSlice({
  name: 'tasks',
  initialState,
  reducers: {
    // clearTasksState Action: Resets the tasks slice to its initial state.
    clearTasksState: (state) => {
      state.tasks = [];
      state.projects = []; // Clears projects data as well upon state reset.
      state.loading = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Reducers for fetchTasksByProjectId thunk.
      .addCase(fetchTasksByProjectId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTasksByProjectId.fulfilled, (state, action: PayloadAction<ITask[]>) => {
        state.loading = false;
        state.tasks = action.payload; // Updates tasks with fetched data.
      })
      .addCase(fetchTasksByProjectId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for fetchMyTasks thunk.
      .addCase(fetchMyTasks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchMyTasks.fulfilled, (state, action: PayloadAction<ITask[]>) => {
        state.loading = false;
        state.tasks = action.payload; // Updates tasks with fetched data.
      })
      .addCase(fetchMyTasks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for fetchTasksByUserId thunk.
      .addCase(fetchTasksByUserId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTasksByUserId.fulfilled, (state, action: PayloadAction<ITask[]>) => {
        state.loading = false;
        state.tasks = action.payload; // Updates tasks with fetched data for a specific user.
      })
      .addCase(fetchTasksByUserId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for createTask thunk.
      .addCase(createTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createTask.fulfilled, (state) => {
        state.loading = false;
        // Task list is re-fetched by the component after successful creation, so no direct mutation here.
      })
      .addCase(createTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for updateTask thunk.
      .addCase(updateTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateTask.fulfilled, (state) => {
        state.loading = false;
        // Task list is re-fetched by the component after successful update, so no direct mutation here.
      })
      .addCase(updateTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for deleteTask thunk.
      .addCase(deleteTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteTask.fulfilled, (state) => {
        state.loading = false;
        // Task list is re-fetched by the component after successful deletion, so no direct mutation here.
      })
      .addCase(deleteTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Reducers for fetchAllProjectsForTasks thunk.
      .addCase(fetchAllProjectsForTasks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllProjectsForTasks.fulfilled, (state, action: PayloadAction<IProject[]>) => {
        state.loading = false;
        state.projects = action.payload; // Stores the fetched projects.
      })
      .addCase(fetchAllProjectsForTasks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

// Exports the action creator for clearing the tasks state.
export const { clearTasksState } = tasksSlice.actions;
// Exports the reducer as the default export for the Redux store.
export default tasksSlice.reducer;