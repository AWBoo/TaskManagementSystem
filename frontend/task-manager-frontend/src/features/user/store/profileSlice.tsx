import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import api from '../../../services/axios'; 
import type { RootState } from '../../../app/store'; 

// IProfile Interface: Defines the structure of a user profile.
export interface IProfile {
  id: string;
  name?: string; 
  email: string;
  status?: string;
  roles?: string[];
}

// IUpdateProfileRequest Interface: Defines the payload for updating a user's profile.
export interface IUpdateProfileRequest {
  name?: string; 
  email?: string; 
}

// IChangePasswordRequest Interface: Defines the payload for a user changing their own password.
export interface IChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

// IAdminChangePasswordRequest Interface: Defines the payload for an admin changing a user's password.
export interface IAdminChangePasswordRequest {
  newPassword: string;
  confirmNewPassword: string;
}

// ProfileState Interface: Defines the structure of the profile slice state.
interface ProfileState {
  profile: IProfile | null; 
  loading: boolean; 
  error: string | null; 
}

// Initial State: Sets the default values for the profile slice.
const initialState: ProfileState = {
  profile: null,
  loading: false,
  error: null,
};


// fetchMyProfile Async Thunk: Fetches the profile of the currently authenticated user.
export const fetchMyProfile = createAsyncThunk<
  IProfile, 
  void, 
  { rejectValue: string } 
>('profile/fetchMyProfile', async (_, { rejectWithValue }) => {
  try {
    const response = await api.get<IProfile>('/api/user/profile');
    return response.data;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch profile.');
  }
});

// fetchUserProfileById Async Thunk: Fetches a specific user's profile by ID (for admin use).
export const fetchUserProfileById = createAsyncThunk<
  IProfile,
  string, // userId as argument.
  { rejectValue: string }
>('profile/fetchUserProfileById', async (userId, { rejectWithValue }) => {
  try {
    const response = await api.get<IProfile>(`/api/admin/users/${userId}`);
    return response.data;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch user profile.');
  }
});

// updateMyProfile Async Thunk: Updates the profile of the currently authenticated user.
export const updateMyProfile = createAsyncThunk<
  IProfile, // Return type (updated profile).
  IUpdateProfileRequest, // Argument type.
  { rejectValue: string; state: RootState }
>('profile/updateMyProfile', async (profileData, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put<IProfile>('/api/user/profile', profileData);
    dispatch(fetchMyProfile()); // Re-fetches profile to ensure state is up-to-date.
    return response.data;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update profile.');
  }
});

// adminUpdateUserProfile Async Thunk: Allows an admin to update a specific user's profile.
export const adminUpdateUserProfile = createAsyncThunk<
  IProfile,
  { userId: string; profileData: IUpdateProfileRequest }, // Argument type.
  { rejectValue: string; state: RootState }
>('profile/adminUpdateUserProfile', async ({ userId, profileData }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put<IProfile>(`/api/admin/users/${userId}/profile`, profileData);
    dispatch(fetchUserProfileById(userId)); // Re-fetches user's profile to ensure state is updated.
    return response.data;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update user profile.');
  }
});

// changeMyPassword Async Thunk: Allows the current user to change their own password.
export const changeMyPassword = createAsyncThunk<
  void, // No specific data is returned on success.
  IChangePasswordRequest,
  { rejectValue: string }
>('profile/changeMyPassword', async (passwordData, { rejectWithValue }) => {
  try {
    await api.post('/api/user/change-password', passwordData);
    return;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to change password.');
  }
});

// adminChangeUserPassword Async Thunk: Allows an admin to change a specific user's password.
export const adminChangeUserPassword = createAsyncThunk<
  void,
  { userId: string; passwordData: IAdminChangePasswordRequest },
  { rejectValue: string }
>('profile/adminChangeUserPassword', async ({ userId, passwordData }, { rejectWithValue }) => {
  try {
    await api.post(`/api/admin/users/${userId}/change-password`, passwordData);
    return;
  } catch (error: any) {
    return rejectWithValue(error.response?.data?.message || 'Failed to change user password.');
  }
});


// profileSlice: Defines the Redux slice for managing profile-related state.
const profileSlice = createSlice({
  name: 'profile',
  initialState,
  reducers: {
    // clearProfileError Action: Clears any error messages in the state.
    clearProfileError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Handles pending, fulfilled, and rejected states for fetchMyProfile.
      .addCase(fetchMyProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchMyProfile.fulfilled, (state, action: PayloadAction<IProfile>) => {
        state.loading = false;
        state.profile = action.payload;
      })
      .addCase(fetchMyProfile.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to fetch profile.';
      })
      // Handles pending, fulfilled, and rejected states for fetchUserProfileById.
      .addCase(fetchUserProfileById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUserProfileById.fulfilled, (state, action: PayloadAction<IProfile>) => {
        state.loading = false;
        state.profile = action.payload;
      })
      .addCase(fetchUserProfileById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to fetch user profile.';
      })
      // Handles pending, fulfilled, and rejected states for updateMyProfile.
      .addCase(updateMyProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateMyProfile.fulfilled, (state) => {
        state.loading = false;
        // Profile is refetched by the thunk, so no direct state update here.
      })
      .addCase(updateMyProfile.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to update profile.';
      })
      // Handles pending, fulfilled, and rejected states for adminUpdateUserProfile.
      .addCase(adminUpdateUserProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(adminUpdateUserProfile.fulfilled, (state) => {
        state.loading = false;
        // Profile is refetched by the thunk, so no direct state update here.
      })
      .addCase(adminUpdateUserProfile.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to update user profile.';
      })
      // Handles pending, fulfilled, and rejected states for changeMyPassword.
      .addCase(changeMyPassword.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(changeMyPassword.fulfilled, (state) => {
        state.loading = false;
        // No profile data needs updating on password change success.
      })
      .addCase(changeMyPassword.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to change password.';
      })
      // Handles pending, fulfilled, and rejected states for adminChangeUserPassword.
      .addCase(adminChangeUserPassword.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(adminChangeUserPassword.fulfilled, (state) => {
        state.loading = false;
        // No profile data needs updating on password change success.
      })
      .addCase(adminChangeUserPassword.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Failed to change user password.';
      });
  },
});

// Exports the action creator for clearing profile errors.
export const { clearProfileError } = profileSlice.actions;
// Exports the reducer as the default export for the Redux store.
export default profileSlice.reducer;