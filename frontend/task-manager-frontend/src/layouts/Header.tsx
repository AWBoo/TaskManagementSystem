import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { logout } from '../auth/AuthSlice';
import type { RootState, AppDispatch } from '../app/store';

import './Css/Header.css';

// Header Functional Component: Displays navigation links and user-specific options.
const Header: React.FC = () => {
  // Selects authentication status and user details from the Redux auth slice.
  const { isAuthenticated, user } = useSelector((state: RootState) => state.auth);
  // Selects the user's profile data, specifically for retrieving the user's ID.
  const myProfile = useSelector((state: RootState) => state.profile.profile);
  console.log(myProfile); // Logs the profile data for debugging purposes.

  const dispatch: AppDispatch = useDispatch();
  const navigate = useNavigate();

  // handleLogout: Dispatches the logout action and redirects to the login page.
  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  // Determines if the logged-in user has the 'Admin' role.
  const isAdmin = user?.roles?.includes('Admin');
  // Extracts the profile ID; used for the "My Profile" link.
  const myProfileId = myProfile?.id;

  // JSX structure for the header.
  return (
    <header className="header-container">
      <div className="header-left">
        <Link to="/" className="header-logo">Task Manager</Link>
      </div>
      <nav className="header-nav">
        {isAuthenticated ? (
          // Renders navigation links for authenticated users.
          <>
            <Link to="/tasks" className="nav-link">My Tasks</Link>
            <Link to="/projects" className="nav-link">Projects</Link>
            {myProfileId && (
              // Displays "My Profile" link only if the profile ID is available.
              <Link to={`/profile/${myProfileId}`} className="nav-link">My Profile</Link>
            )}
            {isAdmin && (
              // Displays "Admin Dashboard" link only if the user is an admin.
              <Link to="/admin-dashboard" className="nav-link">Admin Dashboard</Link>
            )}
            <span className="nav-user-email">{user?.email}</span>
            <button onClick={handleLogout} className="nav-button">Logout</button>
          </>
        ) : (
          // Renders login and register links for unauthenticated users.
          <>
            <Link to="/login" className="nav-link">Login</Link>
            <Link to="/register" className="nav-link">Register</Link>
          </>
        )}
      </nav>
    </header>
  );
};

export default Header;