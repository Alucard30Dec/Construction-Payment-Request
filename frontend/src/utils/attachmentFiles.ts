const ALLOWED_ATTACHMENT_EXTENSIONS = new Set(['.pdf', '.doc', '.docx', '.xls', '.xlsx']);

export const ATTACHMENT_ACCEPT = '.pdf,.doc,.docx,.xls,.xlsx';
export const ATTACHMENT_MAX_SIZE_BYTES = 20 * 1024 * 1024; // 20 MB

export function formatAttachmentFileSize(fileSizeInBytes: number): string {
  if (!Number.isFinite(fileSizeInBytes) || fileSizeInBytes <= 0) {
    return '0 KB';
  }

  if (fileSizeInBytes < 1024) {
    return `${fileSizeInBytes} B`;
  }

  if (fileSizeInBytes < 1024 * 1024) {
    return `${Math.round(fileSizeInBytes / 1024)} KB`;
  }

  return `${(fileSizeInBytes / (1024 * 1024)).toFixed(2)} MB`;
}

export function isPreviewableAttachment(contentType?: string, fileName?: string): boolean {
  const normalizedContentType = contentType?.toLowerCase() ?? '';
  const normalizedFileName = fileName?.toLowerCase() ?? '';

  return normalizedContentType.includes('pdf') || normalizedFileName.endsWith('.pdf');
}

export function validateAttachmentFile(file: File): string | null {
  const dotIndex = file.name.lastIndexOf('.');
  const extension = dotIndex >= 0 ? file.name.slice(dotIndex).toLowerCase() : '';

  if (!ALLOWED_ATTACHMENT_EXTENSIONS.has(extension)) {
    return 'Định dạng tệp không hợp lệ. Chỉ chấp nhận: pdf, doc, docx, xls, xlsx.';
  }

  if (file.size <= 0) {
    return 'Tệp tải lên không hợp lệ.';
  }

  if (file.size > ATTACHMENT_MAX_SIZE_BYTES) {
    return 'Tệp vượt quá dung lượng cho phép (tối đa 20 MB).';
  }

  return null;
}
