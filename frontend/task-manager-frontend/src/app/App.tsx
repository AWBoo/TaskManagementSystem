import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { initializeAuth, logout } from '../auth/AuthSlice';
import { fetchMyProfile } from '../features/user/store/profileSlice';
import type { IUser } from '../auth/AuthInterfaces';
import type { RootState, AppDispatch } from '../app/store';
import Register from '../layouts/Register';
import Home from '../layouts/Home';
import Login from '../layouts/Login';
import MyTasksPage from '../features/tasks/Pages/TasksPage';
import ProjectsPage from '../features/projects/Pages/ProjectsPage';
import ProjectTasksPage from '../features/projects/Pages/ProjectsTasksPage';
import AdminDashboardPage from '../features/admin/pages/AdminDashboardPage';
import ProfilePage from '../features/user/pages/ProfilePage';
import DeactivatedPage from '../features/admin/pages/DeactivatedPage';
import SuspendedPage from '../features/admin/pages/SuspendedPage';
import ProtectedRoute from '../auth/ProtetectedRoute';
import UserAuditHistoryPage from '../features/admin/pages/UserAuditHistoryPage';
import Header from '../layouts/Header';

import './Css/App.css';

function App() {
  // Initialize dispatch with its type for Redux actions.
  const dispatch: AppDispatch = useDispatch();
  // Select the authentication status from the Redux store.
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  // Effect to initialize authentication from local storage on component mount.
  useEffect(() => {
    const storedToken = localStorage.getItem('jwt');
    const storedUser = localStorage.getItem('user');

    // If both token and user data are found, attempt to parse and initialize authentication.
    if (storedToken && storedUser) {
      try {
        const user: IUser = JSON.parse(storedUser);
        dispatch(initializeAuth({ user, token: storedToken }));
      } catch (e) {
        // Log an error and clear corrupted data if parsing fails.
        console.error("Failed to parse user data from localStorage", e);
        dispatch(logout());
      }
    } else {
      // If no token or user data, initialize authentication as null (unauthenticated).
      dispatch(initializeAuth(null));
    }
  }, [dispatch]); // Dependency array ensures effect runs only when dispatch changes.

  // Effect to fetch user profile when authentication status changes to true.
  useEffect(() => {
    if (isAuthenticated) {
      // Fetch the user's profile to populate Redux state,
      // making user ID available for components like the Header.
      dispatch(fetchMyProfile());
    }
  }, [isAuthenticated, dispatch]); // Effect runs when isAuthenticated or dispatch changes.

  return (
    <BrowserRouter>
      <div className="app-container">
        <Header />

        <main className="app-main-content">
          <Routes>
            {/* Publicly accessible routes */}
            <Route path="/" element={<Home />} />
            <Route path="/register" element={<Register />} />
            <Route path="/login" element={<Login />} />
            <Route path="/deactivated" element={<DeactivatedPage />} />
            <Route path="/suspended" element={<SuspendedPage />} />

            {/* Protected routes, accessible only to authenticated users */}
            <Route element={<ProtectedRoute />}>
              <Route path="/tasks" element={<MyTasksPage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/projects/:projectId/tasks" element={<ProjectTasksPage />} />
              {/* Route for displaying a user's own profile or other user profiles */}
              <Route path="/profile/:userId" element={<ProfilePage />} />
            </Route>

            {/* Protected routes, accessible only to Admin users */}
            <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
              <Route path="/admin-dashboard" element={<AdminDashboardPage />} />
              <Route path="/admin/users/:userId/profile" element={<ProfilePage />} />
              <Route path="/admin/users/:userId/tasks" element={<MyTasksPage />} />
              <Route path="/admin/users/:userId/audit" element={<UserAuditHistoryPage />} />
            </Route>

            {/* Fallback route for any undefined paths */}
            <Route path="*" element={<div className="placeholder-message">404 - Page Not Found</div>} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;