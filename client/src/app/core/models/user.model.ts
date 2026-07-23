export interface User {
  id: string;
  email: string;
  displayName: string;
  defaultCurrency: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  user: User;
}
