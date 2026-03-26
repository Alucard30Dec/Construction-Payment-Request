import { apiClient } from './apiClient';
import type {
  CurrentUserPermissions,
  PermissionCatalogItem,
  RoleProfile,
} from '../types';

export interface CreateRoleProfilePayload {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  cloneFromRoleProfileId?: string;
  grantedPermissionCodes: string[];
}

export interface UpdateRoleProfilePayload {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface SaveRolePermissionsPayload {
  grantedPermissionCodes: string[];
}

export const rolePermissionService = {
  async getCatalog(): Promise<PermissionCatalogItem[]> {
    const { data } = await apiClient.get<PermissionCatalogItem[]>('/role-permissions/catalog');
    return data;
  },
  async getProfiles(): Promise<RoleProfile[]> {
    const { data } = await apiClient.get<RoleProfile[]>('/role-permissions/profiles');
    return data;
  },
  async getById(id: string): Promise<RoleProfile> {
    const { data } = await apiClient.get<RoleProfile>(`/role-permissions/profiles/${id}`);
    return data;
  },
  async create(payload: CreateRoleProfilePayload): Promise<RoleProfile> {
    const { data } = await apiClient.post<RoleProfile>('/role-permissions/profiles', payload);
    return data;
  },
  async update(id: string, payload: UpdateRoleProfilePayload): Promise<RoleProfile> {
    const { data } = await apiClient.put<RoleProfile>(`/role-permissions/profiles/${id}`, payload);
    return data;
  },
  async savePermissions(id: string, payload: SaveRolePermissionsPayload): Promise<RoleProfile> {
    const { data } = await apiClient.put<RoleProfile>(
      `/role-permissions/profiles/${id}/permissions`,
      payload,
    );
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/role-permissions/profiles/${id}`);
  },
  async getMyPermissions(): Promise<CurrentUserPermissions> {
    const { data } = await apiClient.get<CurrentUserPermissions>('/role-permissions/me');
    return data;
  },
};
