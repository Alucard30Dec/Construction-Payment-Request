import { apiClient } from './apiClient';
import type { PagedResult, Project } from '../types';

export interface ProjectQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export type ProjectPayload = Omit<Project, 'id' | 'createdAt' | 'updatedAt'>;

export const projectService = {
  async getPaged(params: ProjectQuery): Promise<PagedResult<Project>> {
    const { data } = await apiClient.get<PagedResult<Project>>('/projects', { params });
    return data;
  },
  async getById(id: string): Promise<Project> {
    const { data } = await apiClient.get<Project>(`/projects/${id}`);
    return data;
  },
  async create(payload: ProjectPayload): Promise<Project> {
    const { data } = await apiClient.post<Project>('/projects', payload);
    return data;
  },
  async update(id: string, payload: ProjectPayload): Promise<Project> {
    const { data } = await apiClient.put<Project>(`/projects/${id}`, payload);
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/projects/${id}`);
  },
};
