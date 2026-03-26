import { apiClient } from './apiClient';
import type { ApprovalMatrix, PagedResult } from '../types';

export interface ApprovalMatrixQuery {
  pageNumber?: number;
  pageSize?: number;
  department?: string;
  projectId?: string;
  isActive?: boolean;
}

export interface ApprovalMatrixPayload {
  minAmount: number;
  maxAmount: number;
  requireDirectorApproval: boolean;
  department?: string;
  projectId?: string;
  isActive: boolean;
}

export const approvalMatrixService = {
  async getPaged(params: ApprovalMatrixQuery): Promise<PagedResult<ApprovalMatrix>> {
    const { data } = await apiClient.get<PagedResult<ApprovalMatrix>>('/approvalmatrices', { params });
    return data;
  },
  async create(payload: ApprovalMatrixPayload): Promise<ApprovalMatrix> {
    const { data } = await apiClient.post<ApprovalMatrix>('/approvalmatrices', payload);
    return data;
  },
  async update(id: string, payload: ApprovalMatrixPayload): Promise<ApprovalMatrix> {
    const { data } = await apiClient.put<ApprovalMatrix>(`/approvalmatrices/${id}`, payload);
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/approvalmatrices/${id}`);
  },
};
