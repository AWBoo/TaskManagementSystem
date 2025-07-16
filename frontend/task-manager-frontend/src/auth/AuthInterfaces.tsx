export interface IUser {
  id: string;
  email: string;
  roles?: string[]; 
}

export interface AuthState {
  isAuthenticated: boolean;
  user: IUser | null;
  token: string | null;
  loading: boolean;
  error: string | null;
}

export interface IAuthRequest {
  email: string;
  password: string;
}

export interface IAuthResponse {
  user: IUser;
  token: string;
  message?: string;
}