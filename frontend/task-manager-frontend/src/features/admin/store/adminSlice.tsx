import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import api from '../../../services/axios'; 
import type { RootState } from '../../../app/store'; 

// IRole Interface: Defines the structure for user roles.
export interface IRole {
  id: string;
  name: string;
}

// IDashboardStats Interface: Defines key dashboard statistics.
export interface IDashboardStats {
  totalUsers: number;
  totalProjects: number;
  totalTasks: number;
}

// ITaskStatusCount Interface: Represents task counts by status.
export interface ITaskStatusCount {
  status: string;
  count: number;
}

// IUserTaskCount Interface: Represents task counts for each user.
export interface IUserTaskCount {
  userId: string;
  userEmail: string;
  taskCount: number;
}

// IProjectTaskCount Interface: Represents task counts for each project.
export interface IProjectTaskCount {
  projectId: string;
  projectName: string;
  taskCount: number;
}

// AuditEntry Interface: Defines the structure for audit log entries.
export interface AuditEntry {
  id: string;
  userId: string;
  entityType: string;
  entityId: string;
  changeType: string;
  propertyName?: string;
  oldValue?: string;
  newValue?: string;
  changeTimestamp: string;
  changedByUserId?: string;
  changedByUserEmail?: string;
}

// AdminState Interface: Defines the overall state for the admin slice.
interface AdminState {
  dashboardStats: IDashboardStats | null;
  taskStatusCounts: ITaskStatusCount[];
  userTaskCounts: IUserTaskCount[];
  projectTaskCounts: IProjectTaskCount[];
  roles: IRole[];
  loading: boolean;
  error: string | null;
  latestAuditEntries: AuditEntry[];
  userAuditHistory: AuditEntry[];
  auditLoading: boolean;
  auditError: string | null;
}

// Initial State: Sets the default values for the admin slice.
const initialState: AdminState = {
  dashboardStats: null,
  taskStatusCounts: [],
  userTaskCounts: [],
  projectTaskCounts: [],
  roles: [],
  loading: false,
  error: null,
  latestAuditEntries: [],
  userAuditHistory: [],
  auditLoading: false,
  auditError: null,
};

// Async Thunks: Handle data fetching for various admin dashboards.

// fetchDashboardStats: Retrieves overall dashboard statistics.
export const fetchDashboardStats = createAsyncThunk<
  IDashboardStats,
  void,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchDashboardStats',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IDashboardStats>('/api/admin/dashboard/stats');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch dashboard stats.');
    }
  }
);

// fetchTaskStatusCounts: Retrieves task counts grouped by their status.
export const fetchTaskStatusCounts = createAsyncThunk<
  ITaskStatusCount[],
  void,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchTaskStatusCounts',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<ITaskStatusCount[]>('/api/admin/dashboard/tasks-by-status');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch task status counts.');
    }
  }
);

// fetchUserTaskCounts: Retrieves task counts per user.
export const fetchUserTaskCounts = createAsyncThunk<
  IUserTaskCount[],
  void,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchUserTaskCounts',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IUserTaskCount[]>('/api/admin/dashboard/users-task-counts');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user task counts.');
    }
  }
);

// fetchProjectTaskCounts: Retrieves task counts per project.
export const fetchProjectTaskCounts = createAsyncThunk<
  IProjectTaskCount[],
  void,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchProjectTaskCounts',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IProjectTaskCount[]>('/api/admin/dashboard/projects-task-counts');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch project task counts.');
    }
  }
);

// fetchAllRoles: Retrieves all available roles from the backend.
export const fetchAllRoles = createAsyncThunk<
  IRole[],
  void,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchAllRoles',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get<IRole[]>('/api/admin/roles');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch roles.');
    }
  }
);

// fetchLatestAuditEntries: Retrieves the most recent audit log entries.
export const fetchLatestAuditEntries = createAsyncThunk<
  AuditEntry[],
  number | undefined,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchLatestAuditEntries',
  async (count = 10, { rejectWithValue }) => {
    try {
      const response = await api.get<AuditEntry[]>(`/api/admin/audit/latest?count=${count}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch latest audit entries.');
    }
  }
);

// fetchUserAuditHistory: Retrieves the audit history for a specific user.
export const fetchUserAuditHistory = createAsyncThunk<
  AuditEntry[],
  string,
  { state: RootState; rejectValue: string }
>(
  'admin/fetchUserAuditHistory',
  async (userId, { rejectWithValue }) => {
    try {
      const response = await api.get<AuditEntry[]>(`/api/admin/users/${userId}/history`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user audit history.');
    }
  }
);

// Admin Redux Slice Definition: Configures the Redux slice for admin data.
const adminSlice = createSlice({
  name: 'admin',
  initialState,
  reducers: {
    // clearAdminState: Resets all admin-related state properties.
    clearAdminState: (state) => {
      state.dashboardStats = null;
      state.taskStatusCounts = [];
      state.userTaskCounts = [];
      state.projectTaskCounts = [];
      state.roles = [];
      state.loading = false;
      state.error = null;
      state.latestAuditEntries = [];
      state.userAuditHistory = [];
      state.auditLoading = false;
      state.auditError = null;
    },
  },
  // extraReducers: Handles the lifecycle actions of async thunks.
  extraReducers: (builder) => {
    builder
      // Handles pending, fulfilled, and rejected states for fetchDashboardStats.
      .addCase(fetchDashboardStats.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDashboardStats.fulfilled, (state, action: PayloadAction<IDashboardStats>) => {
        state.loading = false;
        state.dashboardStats = action.payload;
      })
      .addCase(fetchDashboardStats.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchTaskStatusCounts.
      .addCase(fetchTaskStatusCounts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTaskStatusCounts.fulfilled, (state, action: PayloadAction<ITaskStatusCount[]>) => {
        state.loading = false;
        state.taskStatusCounts = action.payload;
      })
      .addCase(fetchTaskStatusCounts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchUserTaskCounts.
      .addCase(fetchUserTaskCounts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUserTaskCounts.fulfilled, (state, action: PayloadAction<IUserTaskCount[]>) => {
        state.loading = false;
        state.userTaskCounts = action.payload;
      })
      .addCase(fetchUserTaskCounts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchProjectTaskCounts.
      .addCase(fetchProjectTaskCounts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProjectTaskCounts.fulfilled, (state, action: PayloadAction<IProjectTaskCount[]>) => {
        state.loading = false;
        state.projectTaskCounts = action.payload;
      })
      .addCase(fetchProjectTaskCounts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchAllRoles.
      .addCase(fetchAllRoles.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllRoles.fulfilled, (state, action: PayloadAction<IRole[]>) => {
        state.loading = false;
        state.roles = action.payload;
      })
      .addCase(fetchAllRoles.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchLatestAuditEntries.
      .addCase(fetchLatestAuditEntries.pending, (state) => {
        state.auditLoading = true;
        state.auditError = null;
      })
      .addCase(fetchLatestAuditEntries.fulfilled, (state, action: PayloadAction<AuditEntry[]>) => {
        state.auditLoading = false;
        state.latestAuditEntries = action.payload;
      })
      .addCase(fetchLatestAuditEntries.rejected, (state, action) => {
        state.auditLoading = false;
        state.auditError = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for fetchUserAuditHistory.
      .addCase(fetchUserAuditHistory.pending, (state) => {
        state.auditLoading = true;
        state.auditError = null;
        state.userAuditHistory = []; // Clears prior history when a new fetch begins.
      })
      .addCase(fetchUserAuditHistory.fulfilled, (state, action: PayloadAction<AuditEntry[]>) => {
        state.auditLoading = false;
        state.userAuditHistory = action.payload;
      })
      .addCase(fetchUserAuditHistory.rejected, (state, action) => {
        state.auditLoading = false;
        state.auditError = action.payload as string;
      });
  },
});

// Exports the action creator for clearAdminState.
export const { clearAdminState } = adminSlice.actions;

// Exports the reducer as the default export for the Redux store.
export default adminSlice.reducer;