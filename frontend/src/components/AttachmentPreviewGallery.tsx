import { useEffect, useMemo, useState } from 'react';
import { Card, Col, Empty, Row, Skeleton, Typography } from 'antd';
import type { Attachment } from '../types';
import { attachmentService } from '../services/attachmentService';
import { formatAttachmentFileSize, isPreviewableAttachment } from '../utils/attachmentFiles';

interface AttachmentPreviewGalleryProps {
  attachments: Attachment[];
  emptyText?: string;
  maxItems?: number;
}

export function AttachmentPreviewGallery({
  attachments,
  emptyText = 'Không có tệp đính kèm để xem trước.',
  maxItems = 8,
}: AttachmentPreviewGalleryProps) {
  const previewableItems = useMemo(
    () =>
      attachments
        .filter((item) => isPreviewableAttachment(item.contentType, item.fileName))
        .slice(0, maxItems),
    [attachments, maxItems],
  );

  const [previewUrlById, setPreviewUrlById] = useState<Record<string, string>>({});
  const [loadingIds, setLoadingIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    let canceled = false;
    const createdUrls: string[] = [];

    setPreviewUrlById({});
    setLoadingIds(new Set(previewableItems.map((item) => item.id)));

    if (previewableItems.length === 0) {
      return () => undefined;
    }

    (async () => {
      const entries: Array<[string, string]> = [];

      for (const item of previewableItems) {
        try {
          const blob = await attachmentService.preview(item.id);
          if (canceled) {
            return;
          }

          const objectUrl = URL.createObjectURL(blob);
          createdUrls.push(objectUrl);
          entries.push([item.id, objectUrl]);
        } catch {
          // Ignore preview failures per file; UI will show fallback message.
        }
      }

      if (canceled) {
        return;
      }

      setPreviewUrlById(Object.fromEntries(entries));
      setLoadingIds(new Set());
    })();

    return () => {
      canceled = true;
      createdUrls.forEach((url) => URL.revokeObjectURL(url));
    };
  }, [previewableItems]);

  if (attachments.length === 0) {
    return <Empty description={emptyText} image={Empty.PRESENTED_IMAGE_SIMPLE} />;
  }

  if (previewableItems.length === 0) {
    return (
      <Typography.Text type="secondary">
        Không có tệp PDF để hiển thị preview trực tiếp.
      </Typography.Text>
    );
  }

  return (
    <Row gutter={[12, 12]}>
      {previewableItems.map((item) => {
        const previewUrl = previewUrlById[item.id];
        const isLoading = loadingIds.has(item.id);
        const isImage = item.contentType?.toLowerCase().startsWith('image/');
        const isPdf =
          item.contentType?.toLowerCase().includes('pdf') || item.fileName.toLowerCase().endsWith('.pdf');

        return (
          <Col key={item.id} xs={24} sm={12} lg={8} xl={6}>
            <Card
              size="small"
              styles={{
                body: {
                  display: 'grid',
                  gap: 8,
                },
              }}
            >
              <div
                style={{
                  height: 180,
                  border: '1px solid #dbe4ee',
                  borderRadius: 8,
                  overflow: 'hidden',
                  background: '#f8fafc',
                  display: 'grid',
                  placeItems: 'center',
                }}
              >
                {isLoading && <Skeleton.Image active style={{ width: 120, height: 120 }} />}

                {!isLoading && previewUrl && isImage && (
                  <img
                    src={previewUrl}
                    alt={item.fileName}
                    style={{ width: '100%', height: '100%', objectFit: 'contain' }}
                  />
                )}

                {!isLoading && previewUrl && isPdf && (
                  <iframe
                    src={previewUrl}
                    title={item.fileName}
                    style={{ width: '100%', height: '100%', border: 0 }}
                  />
                )}

                {!isLoading && !previewUrl && (
                  <Typography.Text type="secondary">Không tải được preview</Typography.Text>
                )}
              </div>

              <Typography.Text strong ellipsis={{ tooltip: item.fileName }}>
                {item.fileName}
              </Typography.Text>
              <Typography.Text type="secondary">{formatAttachmentFileSize(item.fileSize)}</Typography.Text>
            </Card>
          </Col>
        );
      })}
    </Row>
  );
}
