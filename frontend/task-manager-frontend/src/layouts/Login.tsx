import React, { useState } from 'react';
import api from '../services/axios';
import { useNavigate, Link } from 'react-router-dom';
import axios, { AxiosError } from 'axios';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from '../app/store';
import { loginStart, loginSuccess, loginFailure } from '../auth/AuthSlice';
import type { IAuthRequest, IAuthResponse } from '../auth/AuthInterfaces';

import './Css/Login.css';

// Login Functional Component: Handles user authentication and login.
const Login: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();

  // State for managing form input data.
  const [formData, setFormData] = useState<IAuthRequest>({
    email: '',
    password: '',
  });

  // State for displaying login error messages.
  const [error, setError] = useState<string>('');

  // Selects the authentication loading state from the Redux store.
  const authLoading = useSelector((state: RootState) => state.auth.loading);

  // handleChange: Updates form data as user types.
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  // handleSubmit: Handles the login form submission.
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(''); // Clears previous errors.
    dispatch(loginStart()); // Dispatches action to indicate login process has started.

    try {
      // Sends login request to the API.
      const response = await api.post<IAuthResponse>('/api/auth/login', formData);
      const { user, token, message } = response.data;

      // Checks if a token is received, otherwise handles account status message.
      if (!token) {
        const statusMessage = message || 'Login failed due to account status. Please contact support.';
        setError(statusMessage);
        dispatch(loginFailure(statusMessage));
        return;
      }

      // Dispatches success action if login is successful and redirects.
      dispatch(loginSuccess({ user, token }));
      navigate('/');
    } catch (err) {
      // Handles various types of login errors.
      let errorMessage = 'Login failed. Please check your credentials.';
      if (axios.isAxiosError(err)) {
        const axiosError = err as AxiosError<{ message?: string; errors?: any }>;
        if (axiosError.response?.data?.message) {
          errorMessage = axiosError.response.data.message;
        } else if (axiosError.response?.data?.errors) {
          errorMessage = Object.values(axiosError.response.data.errors).flat().join(' ');
        } else if (axiosError.message) {
          errorMessage = axiosError.message;
        }
      }
      setError(errorMessage); // Sets local error for display.
      dispatch(loginFailure(errorMessage)); // Dispatches failure action to Redux.
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2 className="login-title">Login</h2>
        <form onSubmit={handleSubmit} className="login-form">
          <input
            type="email"
            name="email"
            placeholder="Email"
            value={formData.email}
            onChange={handleChange}
            className="login-input"
            required
            disabled={authLoading}
          />
          <input
            type="password"
            name="password"
            placeholder="Password"
            value={formData.password}
            onChange={handleChange}
            className="login-input"
            required
            disabled={authLoading}
          />
          {error && <p className="error-message">{error}</p>}
          <button
            type="submit"
            className="login-button"
            disabled={authLoading}
          >
            {authLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>
        <p className="register-text">
          Don't have an account? <Link to="/register" className="register-link">Register here</Link>
        </p>
      </div>
    </div>
  );
};

export default Login;