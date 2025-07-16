export interface IUser {
  id: string;
  email: string;
}

// Interface for the login/register request bodies.
export interface IAuthRequest {
  email: string;
  password: string;
}

// Interface for the successful authentication response from the backend.
export interface IAuthResponse {
  user: IUser;
  token: string;
  // Optional message included in the authentication response.
  message?: string;
}

// Interface for the shape of the AuthContext value.
export interface IAuthContextType {
  user: IUser | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (userData: IUser, jwtToken: string) => void;
  logout: () => void;
}