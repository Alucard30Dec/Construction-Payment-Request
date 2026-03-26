import { apiClient } from './apiClient';
import type { AuditLog, PagedResult } from '../types';

export interface AuditLogQuery {
  pageNumber?: number;
  pageSize?: number;
  userId?: string;
  action?: string;
  entityName?: string;
  fromDate?: string;
  toDate?: string;
}

export const auditLogService = {
  async getPaged(params: AuditLogQuery): Promise<PagedResult<AuditLog>> {
    const { data } = await apiClient.get<PagedResult<AuditLog>>('/auditlogs', { params });
    return data;
  },
};
