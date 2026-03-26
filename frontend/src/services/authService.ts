import { apiClient } from './apiClient';
import type { AuthResponse, LoginRequest, User } from '../types';

export const authService = {
  async login(payload: LoginRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', payload);
    return data;
  },
  async me(): Promise<User> {
    const { data } = await apiClient.get<User>('/auth/me');
    return data;
  },
  async logout(): Promise<void> {
    await apiClient.post('/auth/logout');
  },
};
