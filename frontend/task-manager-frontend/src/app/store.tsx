import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../auth/AuthSlice.tsx';
import projectsReducer from '../features/projects/store/projectsSlice.tsx';
import tasksReducer from '../features/tasks/store/tasksSlice.tsx';
import usersReducer from '../features/user/store/usersSlice.tsx';
import adminReducer from '../features/admin/store/adminSlice.tsx';
import profileReducer from '../features/user/store/profileSlice.tsx';

// Configure the Redux store with all application reducers.
export const store = configureStore({
  reducer: {
    auth: authReducer,
    projects: projectsReducer,
    tasks: tasksReducer,
    users: usersReducer,
    admin: adminReducer,
    profile: profileReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;