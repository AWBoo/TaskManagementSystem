import React, { useState } from 'react';
import api from '../services/axios';
import { useNavigate, Link } from 'react-router-dom';
import axios, { AxiosError } from 'axios';
import { useDispatch, useSelector } from 'react-redux';
import { loginSuccess, loginFailure, loginStart } from '../auth/AuthSlice';
import type { IAuthRequest, IAuthResponse } from '../auth/AuthInterfaces';
import type { RootState } from '../app/store';

import './Css/Register.css'; // Imports CSS for the Register component.

// Register Functional Component: Handles user registration.
const Register: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();

  // State for managing form input data.
  const [formData, setFormData] = useState<IAuthRequest>({
    email: '',
    password: '',
  });

  // State for displaying registration error messages.
  const [error, setError] = useState<string>('');

  // handleChange: Updates form data as user types.
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  // handleSubmit: Handles the registration form submission.
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(''); // Clears previous errors.
    dispatch(loginStart()); // Dispatches action to set loading state.

    try {
      // Sends registration request to the API.
      const response = await api.post<IAuthResponse>('/api/auth/register', formData);
      const { user, token } = response.data;

      // Dispatches success action if registration is successful and redirects.
      dispatch(loginSuccess({ user, token }));
      navigate('/');
    } catch (err) {
      // Handles various types of registration errors.
      let errorMessage = 'Registration failed. An unexpected error occurred.';
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

  // Selects the authentication loading state from the Redux store.
  const authLoading = useSelector((state: RootState) => state.auth.loading);

  // JSX structure for the Register component.
  return (
    <div className="register-container">
      <div className="register-card">
        <h2 className="register-title">Register</h2>
        <form onSubmit={handleSubmit} className="register-form">
          <input
            type="email"
            name="email"
            placeholder="Email"
            value={formData.email}
            onChange={handleChange}
            className="register-input"
            required
            disabled={authLoading}
          />
          <input
            type="password"
            name="password"
            placeholder="Password"
            value={formData.password}
            onChange={handleChange}
            className="register-input"
            required
            disabled={authLoading}
          />
          {error && <p className="error-message">{error}</p>}
          <button
            type="submit"
            className="register-button"
            disabled={authLoading}
          >
            {authLoading ? 'Registering...' : 'Register'}
          </button>
        </form>
        <p className="login-text">
          Already have an account? <Link to="/login" className="login-link">Login here</Link>
        </p>
      </div>
    </div>
  );
};

export default Register;