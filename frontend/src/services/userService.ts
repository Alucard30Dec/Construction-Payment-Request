import { apiClient } from './apiClient';
import type { PagedResult, RoleProfile, User, UserRole } from '../types';

export interface UserQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  role?: UserRole;
  isActive?: boolean;
}

export interface CreateUserPayload {
  username: string;
  fullName: string;
  email?: string;
  password: string;
  role: UserRole;
  roleProfileId?: string;
  department?: string;
  isActive: boolean;
}

export interface UpdateUserPayload {
  fullName: string;
  email?: string;
  role: UserRole;
  roleProfileId?: string;
  department?: string;
  isActive: boolean;
}

export const userService = {
  async getRoles(): Promise<{ name: UserRole; value: UserRole }[]> {
    const { data } = await apiClient.get<{ name: UserRole; value: UserRole }[]>('/users/roles');
    return data;
  },
  async getPaged(params: UserQuery): Promise<PagedResult<User>> {
    const { data } = await apiClient.get<PagedResult<User>>('/users', { params });
    return data;
  },
  async getRoleProfiles(): Promise<RoleProfile[]> {
    const { data } = await apiClient.get<RoleProfile[]>('/users/role-profiles');
    return data;
  },
  async create(payload: CreateUserPayload): Promise<User> {
    const { data } = await apiClient.post<User>('/users', payload);
    return data;
  },
  async update(id: string, payload: UpdateUserPayload): Promise<User> {
    const { data } = await apiClient.put<User>(`/users/${id}`, payload);
    return data;
  },
  async resetPassword(id: string, newPassword: string): Promise<void> {
    await apiClient.put(`/users/${id}/reset-password`, { newPassword });
  },
};
