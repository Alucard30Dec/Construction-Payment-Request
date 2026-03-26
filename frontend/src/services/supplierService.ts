import { apiClient } from './apiClient';
import type { PagedResult, Supplier } from '../types';

export interface SupplierQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export type SupplierPayload = Omit<Supplier, 'id' | 'createdAt' | 'updatedAt'>;

export const supplierService = {
  async getPaged(params: SupplierQuery): Promise<PagedResult<Supplier>> {
    const { data } = await apiClient.get<PagedResult<Supplier>>('/suppliers', { params });
    return data;
  },
  async getById(id: string): Promise<Supplier> {
    const { data } = await apiClient.get<Supplier>(`/suppliers/${id}`);
    return data;
  },
  async create(payload: SupplierPayload): Promise<Supplier> {
    const { data } = await apiClient.post<Supplier>('/suppliers', payload);
    return data;
  },
  async update(id: string, payload: SupplierPayload): Promise<Supplier> {
    const { data } = await apiClient.put<Supplier>(`/suppliers/${id}`, payload);
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/suppliers/${id}`);
  },
};
