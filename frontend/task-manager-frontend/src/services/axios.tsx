import axios from 'axios';
import { store } from '../app/store';
import { logout } from '../auth/AuthSlice';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercepts outgoing requests to attach the authentication token.
api.interceptors.request.use(
  (config) => {
    // Retrieves the current authentication token from the Redux store.
    const token = store.getState().auth.token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    console.error("Axios Interceptor: Request Error:", error);
    return Promise.reject(error);
  }
);

// Intercepts incoming responses to handle authentication-related errors.
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    console.error("Axios Interceptor: Response Error:", error.response?.status, error.response?.data);

    if (error.response) {
      const { status, data } = error.response;
      const errorMessage = data?.message || '';

      // Handles 403 Forbidden status, specifically for deactivated or suspended accounts.
      if (status === 403) {
        if (errorMessage.includes("deactivated")) {
          console.log("Account deactivated. Redirecting to /deactivated.");
          store.dispatch(logout()); // Logs out the user.
          window.location.href = '/deactivated'; // Redirects to the deactivated page.
          return Promise.reject(error);
        }

        if (errorMessage.includes("suspended")) {
          console.log("Account suspended. Redirecting to /suspended.");
          store.dispatch(logout()); // Logs out the user.
          window.location.href = '/suspended'; // Redirects to the suspended page.
          return Promise.reject(error);
        }
      }

      // Handles generic 401 Unauthorized errors, such as expired or invalid tokens.
      if (status === 401) {
        console.error("Axios Interceptor: Unauthorized. Logging out.");
        store.dispatch(logout()); // Dispatches logout to clear authentication state.
      }
    }
    return Promise.reject(error);
  }
);

export default api;