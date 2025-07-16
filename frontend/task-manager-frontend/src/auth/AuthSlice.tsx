import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { AuthState, IUser } from './AuthInterfaces';

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  token: null,
  loading: true,
  error: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginStart: (state) => {
      state.loading = true;
      state.error = null;
    },
    loginSuccess: (state, action: PayloadAction<{ user: IUser; token: string }>) => {
      state.isAuthenticated = true;
      state.user = action.payload.user;
      state.token = action.payload.token;
      state.loading = false;
      state.error = null;
      localStorage.setItem('user', JSON.stringify(action.payload.user));
      localStorage.setItem('jwt', action.payload.token);
    },
    loginFailure: (state, action: PayloadAction<string>) => {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.loading = false;
      state.error = action.payload;
      localStorage.removeItem('user');
      localStorage.removeItem('jwt');
    },
    logout: (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.loading = false;
      state.error = null;
      localStorage.removeItem('user');
      localStorage.removeItem('jwt');
    },
    // Initializes authentication state from local storage on app load.
    initializeAuth: (state, action: PayloadAction<{ user: IUser; token: string } | null>) => {
      if (action.payload) {
        state.isAuthenticated = true;
        state.user = action.payload.user;
        state.token = action.payload.token;
      } else {
        state.isAuthenticated = false;
        state.user = null;
        state.token = null;
      }
      state.loading = false;
    },
  },
});

export const { loginStart, loginSuccess, loginFailure, logout, initializeAuth } = authSlice.actions;
export default authSlice.reducer;