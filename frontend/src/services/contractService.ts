import { apiClient } from './apiClient';
import type { Contract, PagedResult } from '../types';

export interface ContractQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  supplierId?: string;
  projectId?: string;
  isActive?: boolean;
}

export type ContractPayload = Omit<
  Contract,
  'id' | 'createdAt' | 'updatedAt' | 'supplierName' | 'projectName' | 'attachmentCount' | 'attachments'
>;

export const contractService = {
  async getPaged(params: ContractQuery): Promise<PagedResult<Contract>> {
    const { data } = await apiClient.get<PagedResult<Contract>>('/contracts', { params });
    return data;
  },
  async getById(id: string): Promise<Contract> {
    const { data } = await apiClient.get<Contract>(`/contracts/${id}`);
    return data;
  },
  async create(payload: ContractPayload): Promise<Contract> {
    const { data } = await apiClient.post<Contract>('/contracts', payload);
    return data;
  },
  async update(id: string, payload: ContractPayload): Promise<Contract> {
    const { data } = await apiClient.put<Contract>(`/contracts/${id}`, payload);
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/contracts/${id}`);
  },
};
