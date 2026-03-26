import { apiClient } from './apiClient';
import type {
  PagedResult,
  PaymentRequest,
  PaymentRequestDetail,
  PaymentRequestStatus,
  PaymentMethod,
  PaymentStatus,
} from '../types';

export interface PaymentRequestQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  projectId?: string;
  supplierId?: string;
  status?: PaymentRequestStatus;
  fromDate?: string;
  toDate?: string;
}

export interface PaymentRequestPayload {
  requestCode?: string;
  title: string;
  projectId: string;
  supplierId: string;
  contractId?: string;
  requestType: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  description?: string;
  amountBeforeVat: number;
  vatRate: number;
  advanceDeduction: number;
  retentionAmount: number;
  otherDeduction: number;
  paymentMethod: PaymentMethod;
  notes?: string;
}

export interface WorkflowActionPayload {
  comment?: string;
}

export interface PaymentConfirmationPayload {
  paymentDate: string;
  paymentReferenceNumber?: string;
  bankTransactionNumber?: string;
  paidAmount: number;
  accountingNote?: string;
  paymentStatus: PaymentStatus;
}

export const paymentRequestService = {
  async getPaged(params: PaymentRequestQuery): Promise<PagedResult<PaymentRequest>> {
    const { data } = await apiClient.get<PagedResult<PaymentRequest>>('/paymentrequests', { params });
    return data;
  },
  async getById(id: string): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.get<PaymentRequestDetail>(`/paymentrequests/${id}`);
    return data;
  },
  async create(payload: PaymentRequestPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>('/paymentrequests', payload);
    return data;
  },
  async update(id: string, payload: PaymentRequestPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.put<PaymentRequestDetail>(`/paymentrequests/${id}`, payload);
    return data;
  },
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/paymentrequests/${id}`);
  },
  async submit(id: string, payload: WorkflowActionPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>(`/paymentrequests/${id}/submit`, payload);
    return data;
  },
  async approve(id: string, payload: WorkflowActionPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>(`/paymentrequests/${id}/approve`, payload);
    return data;
  },
  async reject(id: string, payload: WorkflowActionPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>(`/paymentrequests/${id}/reject`, payload);
    return data;
  },
  async returnForEdit(id: string, payload: WorkflowActionPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>(`/paymentrequests/${id}/return`, payload);
    return data;
  },
  async confirmPayment(id: string, payload: PaymentConfirmationPayload): Promise<PaymentRequestDetail> {
    const { data } = await apiClient.post<PaymentRequestDetail>(`/paymentrequests/${id}/confirm-payment`, payload);
    return data;
  },
};
