import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import api from '../../../services/axios'; 
import type { IProject, ICreateProjectRequest, IUpdateProjectRequest, IFetchProjectsParams } from './ProjectInterfaces';
import type { RootState } from '../../../app/store'; 

// ProjectsState Interface: Defines the structure of the projects slice state.
interface ProjectsState {
  projects: IProject[];
  loading: boolean;
  error: string | null;
}

// Initial State: Sets the default values for the projects slice.
const initialState: ProjectsState = {
  projects: [],
  loading: false,
  error: null,
};

// fetchProjects Async Thunk: Fetches all projects, optionally filtered by creation date range.
export const fetchProjects = createAsyncThunk<
  IProject[], // Type of the fulfilled payload.
  IFetchProjectsParams | undefined, // Optional parameters for date filtering.
  { state: RootState; rejectValue: string } // ThunkAPI types for state access and error handling.
>(
  'projects/fetchProjects',
  async (params, { rejectWithValue }) => {
    try {
      let url = '/api/projects';
      const queryParams = new URLSearchParams();

      // Appends startDate query parameter if provided.
      if (params?.startDate) {
        queryParams.append('startDate', params.startDate);
      }
      // Appends endDate query parameter if provided.
      if (params?.endDate) {
        queryParams.append('endDate', params.endDate);
      }

      // Constructs the URL with query parameters if any are present.
      if (queryParams.toString()) {
        url = `${url}?${queryParams.toString()}`;
      }

      const response = await api.get<IProject[]>(url);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch projects');
    }
  }
);

// createProject Async Thunk: Creates a new project.
export const createProject = createAsyncThunk<IProject, ICreateProjectRequest, { state: RootState, rejectValue: string }>(
  'projects/createProject',
  async (projectData, { rejectWithValue }) => {
    try {
      const response = await api.post<IProject>('/api/projects', projectData);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create project');
    }
  }
);

// updateProject Async Thunk: Updates an existing project.
export const updateProject = createAsyncThunk<IProject, { id: string; data: IUpdateProjectRequest }, { state: RootState, rejectValue: string }>(
  'projects/updateProject',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await api.put<IProject>(`/api/projects/${id}`, data);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update project');
    }
  }
);

// deleteProject Async Thunk: Deletes a project by its ID.
export const deleteProject = createAsyncThunk<string, string, { state: RootState, rejectValue: string }>(
  'projects/deleteProject',
  async (id, { rejectWithValue }) => {
    try {
      await api.delete(`/api/projects/${id}`);
      return id; // Returns the ID of the successfully deleted project.
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete project');
    }
  }
);

// projectsSlice: Defines the Redux slice for managing project-related state.
const projectsSlice = createSlice({
  name: 'projects',
  initialState,
  reducers: {
    // clearProjectsState Action: Resets the projects slice to its initial state.
    clearProjectsState: (state) => {
      state.projects = [];
      state.loading = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Handles pending, fulfilled, and rejected states for fetchProjects.
      .addCase(fetchProjects.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProjects.fulfilled, (state, action: PayloadAction<IProject[]>) => {
        state.loading = false;
        state.projects = action.payload; // Updates state with fetched projects.
      })
      .addCase(fetchProjects.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for createProject.
      .addCase(createProject.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createProject.fulfilled, (state, action: PayloadAction<IProject>) => {
        state.loading = false;
        state.projects.push(action.payload); // Adds the newly created project to the list.
      })
      .addCase(createProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for updateProject.
      .addCase(updateProject.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateProject.fulfilled, (state, action: PayloadAction<IProject>) => {
        state.loading = false;
        // Finds and updates the existing project in the state.
        const index = state.projects.findIndex(p => p.id === action.payload.id);
        if (index !== -1) {
          state.projects[index] = action.payload;
        }
      })
      .addCase(updateProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Handles pending, fulfilled, and rejected states for deleteProject.
      .addCase(deleteProject.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteProject.fulfilled, (state, action: PayloadAction<string>) => {
        state.loading = false;
        // Removes the deleted project from the state.
        state.projects = state.projects.filter(p => p.id !== action.payload);
      })
      .addCase(deleteProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

// Exports the action creator for clearProjectsState.
export const { clearProjectsState } = projectsSlice.actions;
// Exports the reducer as the default export for the Redux store.
export default projectsSlice.reducer;