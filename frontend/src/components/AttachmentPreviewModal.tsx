import { Modal, Typography } from 'antd';

interface AttachmentPreviewModalProps {
  open: boolean;
  fileName?: string;
  contentType?: string;
  previewUrl?: string;
  onClose: () => void;
}

export function AttachmentPreviewModal({
  open,
  fileName,
  contentType,
  previewUrl,
  onClose,
}: AttachmentPreviewModalProps) {
  const normalizedContentType = contentType?.toLowerCase() ?? '';
  const normalizedFileName = fileName?.toLowerCase() ?? '';
  const isImage = normalizedContentType.startsWith('image/');
  const isPdf = normalizedContentType.includes('pdf') || normalizedFileName.endsWith('.pdf');

  return (
    <Modal
      open={open}
      title={fileName ?? 'Xem trước tệp'}
      width="90vw"
      footer={null}
      onCancel={onClose}
      destroyOnClose
    >
      {!previewUrl && <Typography.Text>Không có dữ liệu để xem trước.</Typography.Text>}

      {previewUrl && isImage && (
        <div style={{ textAlign: 'center' }}>
          <img
            src={previewUrl}
            alt={fileName ?? 'preview'}
            style={{ maxWidth: '100%', maxHeight: '70vh', objectFit: 'contain' }}
          />
        </div>
      )}

      {previewUrl && isPdf && (
        <iframe
          src={previewUrl}
          title={fileName ?? 'preview'}
          style={{ width: '100%', height: '70vh', border: 0 }}
        />
      )}

      {previewUrl && !isImage && !isPdf && (
        <Typography.Text>
          Định dạng này chưa hỗ trợ xem trước trực tiếp. Vui lòng tải file để xem đầy đủ.
        </Typography.Text>
      )}
    </Modal>
  );
}
