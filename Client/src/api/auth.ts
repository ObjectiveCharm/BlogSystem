import apiClient from './client';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/login', data);
    return response.data;
  },

  refreshToken: async (refreshToken: string): Promise<{ token: string }> => {
    const response = await apiClient.post<{ token: string }>('/refresh-token', {
      refreshToken,
    });
    return response.data;
  },

  changePassword: async (data: ChangePasswordRequest): Promise<string> => {
    const response = await apiClient.post<string>('/change-password', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<{ id: string }> => {
    // Registration flow:
    // 1. Create user with POST /user/{id}
    // 2. Create user credential with POST /usercredential/{id}
    const userId = crypto.randomUUID();
    
    // Create user
    await apiClient.post(`/user/${userId}`, {
      username: data.username,
    });
    
    // Create user credential
    await apiClient.post(`/usercredential/${userId}`, {
      email: data.email,
      password: data.password,
    });
    
    return { id: userId };
  },
};
