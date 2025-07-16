import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import api from '../../../services/axios'; 
import type { RootState } from '../../../app/store'; 
import type { ITask } from '../../tasks/store/TaskInterfaces'; 
import type { IUserDashboardData } from './DashboardIterface';

// IUserBasic Interface: Defines the basic structure of a user.
export interface IUserBasic {
  id: string;
  email: string;
  roles: string[];
  projectCount: number; 
  taskCount: number; 
  name: string | null;
  status: string;
}

// UsersState Interface: Defines the structure of the users slice state.
interface UsersState {
  users: IUserBasic[];
  userTasks: ITask[]; 
  loading: boolean; 
  error: string | null; 
  userDashboardData: IUserDashboardData | null;
  dashboardLoading: boolean; 
  dashboardError: string | null; 
}

// Initial State: Sets the default values for the users slice.
const initialState: UsersState = {
  users: [],
  userTasks: [],
  loading: false,
  error: null,
  userDashboardData: null,
  dashboardLoading: false,
  dashboardError: null,
};

//Fetches all users from the backend.
export const fetchAllUsers = createAsyncThunk<
  IUserBasic[], 
  void, 
  { state: RootState; rejectValue: string } 
>(
  'users/fetchAllUsers',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IUserBasic[]>('/api/admin/users');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch users.');
    }
  }
);

// assignRoleToUser Async Thunk: Assigns a specified role to a user.
export const assignRoleToUser = createAsyncThunk<
  { userId: string; roleName: string; message: string }, // Return type.
  { userId: string; roleName: string }, // Arguments: user ID and role name.
  { state: RootState; rejectValue: string }
>(
  'users/assignRoleToUser',
  async ({ userId, roleName }, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.post<{ message: string }>(`/api/admin/users/${userId}/roles/assign`, { roleName });
      dispatch(fetchAllUsers()); // Re-fetches all users to update the UI with new role.
      return { userId, roleName, message: response.data.message };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || `Failed to assign role '${roleName}' to user ${userId}.`);
    }
  }
);

// removeRoleFromUser Async Thunk: Removes a specified role from a user.
export const removeRoleFromUser = createAsyncThunk<
  { userId: string; roleName: string; message: string }, // Return type.
  { userId: string; roleName: string }, // Arguments: user ID and role name.
  { state: RootState; rejectValue: string }
>(
  'users/removeRoleFromUser',
  async ({ userId, roleName }, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.post<{ message: string }>(`/api/admin/users/${userId}/roles/remove`, { roleName });
      dispatch(fetchAllUsers()); // Re-fetches all users to update the UI.
      return { userId, roleName, message: response.data.message };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || `Failed to remove role '${roleName}' from user ${userId}.`);
    }
  }
);

// deleteUser Async Thunk: Deletes a user by their ID.
export const deleteUser = createAsyncThunk<
  string, // Return type (the ID of the deleted user).
  string, // Argument type: userId.
  { state: RootState; rejectValue: string }
>(
  'users/deleteUser',
  async (userId, { rejectWithValue, dispatch }) => {
    try {
      await api.delete(`/api/admin/users/${userId}`);
      dispatch(fetchAllUsers()); // Re-fetches all users to update the UI.
      return userId;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || `Failed to delete user ${userId}.`);
    }
  }
);

// updateUserStatus Async Thunk: Updates a user's account status.
export const updateUserStatus = createAsyncThunk<
  { userId: string; newStatus: string; message: string }, // Return type.
  { userId: string; newStatus: string }, // Arguments: user ID and new status.
  { state: RootState; rejectValue: string }
>(
  'users/updateUserStatus',
  async ({ userId, newStatus }, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.put<{ message: string }>(`/api/admin/users/${userId}/status`, { newStatus });
      dispatch(fetchAllUsers()); // Re-fetches all users to update the UI.
      return { userId, newStatus, message: response.data.message };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || `Failed to update status for user ${userId}.`);
    }
  }
);

// fetchTasksByUserId Async Thunk: Fetches tasks assigned to a specific user.
export const fetchTasksByUserId = createAsyncThunk<
  ITask[], // Return type.
  string, // Argument type: userId.
  { state: RootState; rejectValue: string }
>(
  'users/fetchTasksByUserId',
  async (userId, { rejectWithValue }) => {
    try {
      const response = await api.get<ITask[]>(`/api/admin/users/${userId}/tasks`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || `Failed to fetch tasks for user ${userId}.`);
    }
  }
);

// fetchUserDashboardData Async Thunk: Fetches personalized dashboard data for the current user.
export const fetchUserDashboardData = createAsyncThunk<
  IUserDashboardData, // Expected return type.
  void, // No arguments are needed as user ID is derived from authentication context.
  { state: RootState; rejectValue: string }
>(
  'users/fetchUserDashboardData',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IUserDashboardData>('/api/user/dashboard-summary');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch dashboard data.');
    }
  }
);

// Users Redux Slice Definition: Configures the Redux slice for managing user-related state.
const usersSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    // clearUsersState Action: Resets the users slice to its initial state.
    clearUsersState: (state) => {
      state.users = [];
      state.userTasks = [];
      state.loading = false;
      state.error = null;
      state.userDashboardData = null; // Clears dashboard data.
      state.dashboardLoading = false;
      state.dashboardError = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Handles pending, fulfilled, and rejected states for fetchAllUsers.
      .addCase(fetchAllUsers.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllUsers.fulfilled, (state, action: PayloadAction<IUserBasic[]>) => {
        state.loading = false;
        state.users = action.payload;
      })
      .addCase(fetchAllUsers.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for assignRoleToUser.
      .addCase(assignRoleToUser.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(assignRoleToUser.fulfilled, (state) => { state.loading = false; })
      .addCase(assignRoleToUser.rejected, (state, action) => { state.loading = false; state.error = action.payload as string; })
      // Handles pending, fulfilled, and rejected states for removeRoleFromUser.
      .addCase(removeRoleFromUser.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(removeRoleFromUser.fulfilled, (state) => { state.loading = false; })
      .addCase(removeRoleFromUser.rejected, (state, action) => { state.loading = false; state.error = action.payload as string; })
      // Handles pending, fulfilled, and rejected states for deleteUser.
      .addCase(deleteUser.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(deleteUser.fulfilled, (state) => { state.loading = false; })
      .addCase(deleteUser.rejected, (state, action) => { state.loading = false; state.error = action.payload as string; })
      // Handles pending, fulfilled, and rejected states for updateUserStatus.
      .addCase(updateUserStatus.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(updateUserStatus.fulfilled, (state) => { state.loading = false; })
      .addCase(updateUserStatus.rejected, (state, action) => { state.loading = false; state.error = action.payload as string; })
      // Handles pending, fulfilled, and rejected states for fetchTasksByUserId.
      .addCase(fetchTasksByUserId.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.userTasks = []; // Clears previous tasks when fetching new ones.
      })
      .addCase(fetchTasksByUserId.fulfilled, (state, action: PayloadAction<ITask[]>) => {
        state.loading = false;
        state.userTasks = action.payload; // Updates with tasks for the specific user.
      })
      .addCase(fetchTasksByUserId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
        state.userTasks = []; // Clears tasks on error.
      })
      // Handles pending, fulfilled, and rejected states for fetchUserDashboardData.
      .addCase(fetchUserDashboardData.pending, (state) => {
        state.dashboardLoading = true;
        state.dashboardError = null;
        state.userDashboardData = null; // Clears previous dashboard data.
      })
      .addCase(fetchUserDashboardData.fulfilled, (state, action: PayloadAction<IUserDashboardData>) => {
        state.dashboardLoading = false;
        state.userDashboardData = action.payload; // Updates with fetched dashboard data.
      })
      .addCase(fetchUserDashboardData.rejected, (state, action) => {
        state.dashboardLoading = false;
        state.dashboardError = action.payload as string;
        state.userDashboardData = null; // Clears data on error.
      });
  },
});

// Exports the action creator for clearing the users state.
export const { clearUsersState } = usersSlice.actions;
// Exports the reducer as the default export for the Redux store.
export default usersSlice.reducer;