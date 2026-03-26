import { apiClient } from './apiClient';
import type { Attachment } from '../types';

function toFormData(file: File): FormData {
  const formData = new FormData();
  formData.append('file', file);
  return formData;
}

export const attachmentService = {
  async getByPaymentRequestId(paymentRequestId: string): Promise<Attachment[]> {
    const { data } = await apiClient.get<Attachment[]>(`/payment-requests/${paymentRequestId}/attachments`);
    return data;
  },
  async uploadPaymentRequestAttachment(paymentRequestId: string, file: File): Promise<Attachment> {
    const { data } = await apiClient.post<Attachment>(
      `/payment-requests/${paymentRequestId}/attachments`,
      toFormData(file),
    );

    return data;
  },
  async getByContractId(contractId: string): Promise<Attachment[]> {
    const { data } = await apiClient.get<Attachment[]>(`/contracts/${contractId}/attachments`);
    return data;
  },
  async uploadContractAttachment(contractId: string, file: File): Promise<Attachment> {
    const { data } = await apiClient.post<Attachment>(
      `/contracts/${contractId}/attachments`,
      toFormData(file),
    );

    return data;
  },
  async getByPaymentConfirmation(paymentRequestId: string): Promise<Attachment[]> {
    const { data } = await apiClient.get<Attachment[]>(
      `/payment-requests/${paymentRequestId}/payment-confirmation/attachments`,
    );
    return data;
  },
  async uploadPaymentConfirmationAttachment(paymentRequestId: string, file: File): Promise<Attachment> {
    const { data } = await apiClient.post<Attachment>(
      `/payment-requests/${paymentRequestId}/payment-confirmation/attachments`,
      toFormData(file),
    );

    return data;
  },
  async delete(attachmentId: string): Promise<void> {
    await apiClient.delete(`/attachments/${attachmentId}`);
  },
  async download(attachmentId: string, fileName: string): Promise<void> {
    const { data } = await apiClient.get<Blob>(`/attachments/${attachmentId}/download`, {
      responseType: 'blob',
    });

    const blobUrl = window.URL.createObjectURL(data);
    const anchor = document.createElement('a');
    anchor.href = blobUrl;
    anchor.download = fileName;
    anchor.style.display = 'none';

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    window.URL.revokeObjectURL(blobUrl);
  },
  async preview(attachmentId: string): Promise<Blob> {
    const { data } = await apiClient.get<Blob>(`/attachments/${attachmentId}/preview`, {
      responseType: 'blob',
    });
    return data;
  },
};
