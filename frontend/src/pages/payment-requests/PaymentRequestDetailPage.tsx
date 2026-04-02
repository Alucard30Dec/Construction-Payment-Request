import {
  ArrowLeftOutlined,
  DownloadOutlined,
  EditOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Descriptions, Space, Table, Typography, message } from 'antd';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { paymentRequestService } from '../../services/paymentRequestService';
import { attachmentService } from '../../services/attachmentService';
import { StatusBadge } from '../../components/StatusBadge';
import { getErrorMessage } from '../../utils/apiError';
import type { Attachment, PaymentRequestApprovalHistory } from '../../types';
import { formatCurrency, formatDate, formatDateTime } from '../../utils/formatters';
import { AttachmentPreviewModal } from '../../components/AttachmentPreviewModal';
import { AttachmentPreviewGallery } from '../../components/AttachmentPreviewGallery';
import { formatAttachmentFileSize } from '../../utils/attachmentFiles';

export function PaymentRequestDetailPage() {
  const params = useParams<{ id: string }>();
  const navigate = useNavigate();
  const id = params.id as string;
  const [previewState, setPreviewState] = useState<{
    open: boolean;
    objectUrl?: string;
    fileName?: string;
    contentType?: string;
  }>({ open: false });

  const query = useQuery({
    queryKey: ['payment-request-detail', id],
    queryFn: () => paymentRequestService.getById(id),
  });

  const item = query.data;

  const openAttachmentPreview = async (record: Attachment): Promise<void> => {
    try {
      const blob = await attachmentService.preview(record.id);
      const objectUrl = URL.createObjectURL(blob);
      setPreviewState({
        open: true,
        objectUrl,
        fileName: record.fileName,
        contentType: record.contentType,
      });
    } catch (error) {
      message.error(getErrorMessage(error));
    }
  };

  const downloadAttachment = async (record: Attachment): Promise<void> => {
    try {
      await attachmentService.download(record.id, record.fileName);
    } catch (error) {
      message.error(getErrorMessage(error));
    }
  };

  return (
    <div className="page-stack">
      <div className="page-header">
        <div className="page-header__content">
          <Typography.Title level={3} style={{ margin: 0 }}>
            Chi tiết hồ sơ thanh toán
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Thông tin chi tiết được tách khối rõ ràng, ưu tiên khả năng đọc và không cuộn ngang trên mobile.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Space wrap>
            <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/payment-requests')}>
              Quay lại
            </Button>
            {item && ['Draft', 'ReturnedForEdit', 'Rejected'].includes(item.currentStatus) && (
              <Button
                type="primary"
                icon={<EditOutlined />}
                onClick={() => navigate(`/payment-requests/${item.id}/edit`)}
              >
                Sửa hồ sơ
              </Button>
            )}
          </Space>
        </div>
      </div>

      <Card className="page-card descriptions-responsive" loading={query.isLoading}>
        {item && (
          <>
            <Descriptions title="Thông tin chung" bordered column={{ xs: 1, sm: 1, md: 2 }} size="small">
              <Descriptions.Item label="Mã hồ sơ">{item.requestCode}</Descriptions.Item>
              <Descriptions.Item label="Trạng thái">
                <StatusBadge status={item.currentStatus} />
              </Descriptions.Item>
              <Descriptions.Item label="Tiêu đề" span={2}>
                {item.title}
              </Descriptions.Item>
              <Descriptions.Item label="Dự án">{item.projectName}</Descriptions.Item>
              <Descriptions.Item label="Nhà cung cấp">{item.supplierName}</Descriptions.Item>
              <Descriptions.Item label="Hợp đồng">{item.contractName ?? '-'}</Descriptions.Item>
              <Descriptions.Item label="Loại đề nghị">{item.requestType}</Descriptions.Item>
              <Descriptions.Item label="Người tạo">{item.createdByUsername}</Descriptions.Item>
              <Descriptions.Item label="Ngày tạo">{formatDateTime(item.createdAt)}</Descriptions.Item>
              <Descriptions.Item label="Mô tả" span={2}>
                {item.description ?? '-'}
              </Descriptions.Item>
            </Descriptions>

            <Descriptions
              title="Thông tin hóa đơn"
              bordered
              column={{ xs: 1, sm: 1, md: 3 }}
              size="small"
              style={{ marginTop: 16 }}
            >
              <Descriptions.Item label="Số hóa đơn">{item.invoiceNumber}</Descriptions.Item>
              <Descriptions.Item label="Ngày hóa đơn">{formatDate(item.invoiceDate)}</Descriptions.Item>
              <Descriptions.Item label="Hạn thanh toán">{formatDate(item.dueDate)}</Descriptions.Item>
            </Descriptions>

            <Descriptions
              title="Giá trị thanh toán"
              bordered
              column={{ xs: 1, sm: 1, md: 3 }}
              size="small"
              style={{ marginTop: 16 }}
            >
              <Descriptions.Item label="Trước VAT">{formatCurrency(item.amountBeforeVat)}</Descriptions.Item>
              <Descriptions.Item label="VAT (%)">{item.vatRate}</Descriptions.Item>
              <Descriptions.Item label="Tiền VAT">{formatCurrency(item.vatAmount)}</Descriptions.Item>
              <Descriptions.Item label="Sau VAT">{formatCurrency(item.amountAfterVat)}</Descriptions.Item>
              <Descriptions.Item label="Tạm ứng">{formatCurrency(item.advanceDeduction)}</Descriptions.Item>
              <Descriptions.Item label="Giữ lại">{formatCurrency(item.retentionAmount)}</Descriptions.Item>
              <Descriptions.Item label="Khấu trừ khác">{formatCurrency(item.otherDeduction)}</Descriptions.Item>
              <Descriptions.Item label="Đề nghị thanh toán" span={2}>
                <strong>{formatCurrency(item.requestedAmount)}</strong>
              </Descriptions.Item>
            </Descriptions>
          </>
        )}
      </Card>

      <Card className="page-card" title="Bản xem trước hồ sơ đính kèm" loading={query.isLoading}>
        <AttachmentPreviewGallery
          attachments={item?.attachments ?? []}
          emptyText="Chưa có file hồ sơ thanh toán để xem trước."
          maxItems={12}
        />
      </Card>

      <Card className="page-card" title="File đính kèm" loading={query.isLoading}>
        <Table<Attachment>
          className="responsive-table"
          size="small"
          rowKey="id"
          dataSource={item?.attachments ?? []}
          pagination={false}
          scroll={{ x: 620 }}
          columns={[
            { title: 'Tên tệp', dataIndex: 'fileName', key: 'fileName' },
            {
              title: 'Dung lượng',
              key: 'fileSize',
              responsive: ['sm'],
              render: (_: unknown, record: Attachment) => formatAttachmentFileSize(record.fileSize),
            },
            {
              title: 'Thao tác',
              key: 'actions',
              render: (_: unknown, record: Attachment) => (
                <Space wrap className="table-actions">
                  <Button icon={<EyeOutlined />} onClick={() => void openAttachmentPreview(record)} />
                  <Button icon={<DownloadOutlined />} onClick={() => void downloadAttachment(record)} />
                </Space>
              ),
            },
          ]}
        />
      </Card>

      <Card className="page-card" title="Lịch sử duyệt" loading={query.isLoading}>
        <Table<PaymentRequestApprovalHistory>
          className="responsive-table"
          size="small"
          rowKey="id"
          dataSource={item?.approvalHistories ?? []}
          pagination={false}
          scroll={{ x: 860 }}
          columns={[
            { title: 'Bước', dataIndex: 'stepOrder', key: 'stepOrder', responsive: ['sm'] },
            {
              title: 'Người xử lý',
              key: 'approver',
              render: (_: unknown, record: PaymentRequestApprovalHistory) =>
                `${record.approverFullName} (${record.approverUsername})`,
            },
            { title: 'Hành động', dataIndex: 'action', key: 'action', responsive: ['md'] },
            {
              title: 'Thời gian',
              key: 'actionAt',
              responsive: ['lg'],
              render: (_: unknown, record: PaymentRequestApprovalHistory) => formatDateTime(record.actionAt),
            },
            { title: 'Ghi chú', dataIndex: 'comment', key: 'comment' },
          ]}
        />
      </Card>

      {item?.paymentConfirmation && (
        <Card className="page-card" title="Thông tin thanh toán kế toán">
          <Descriptions bordered size="small" column={{ xs: 1, sm: 1, md: 2 }}>
            <Descriptions.Item label="Ngày thanh toán">
              {formatDate(item.paymentConfirmation.paymentDate)}
            </Descriptions.Item>
            <Descriptions.Item label="Trạng thái thanh toán">
              {item.paymentConfirmation.paymentStatus}
            </Descriptions.Item>
            <Descriptions.Item label="Số tham chiếu">
              {item.paymentConfirmation.paymentReferenceNumber ?? '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Mã giao dịch ngân hàng">
              {item.paymentConfirmation.bankTransactionNumber ?? '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Số tiền thanh toán" span={2}>
              {formatCurrency(item.paymentConfirmation.paidAmount)}
            </Descriptions.Item>
            <Descriptions.Item label="Ghi chú kế toán" span={2}>
              {item.paymentConfirmation.accountingNote ?? '-'}
            </Descriptions.Item>
          </Descriptions>

          <div style={{ marginTop: 16 }}>
            <Typography.Title level={5} style={{ marginBottom: 12 }}>
              Bản xem trước chứng từ kế toán
            </Typography.Title>
            <AttachmentPreviewGallery
              attachments={item.paymentConfirmation.attachments ?? []}
              emptyText="Chưa có tệp hóa đơn/chứng từ kế toán để xem trước."
              maxItems={12}
            />
          </div>

          <Table<Attachment>
            className="responsive-table"
            style={{ marginTop: 16 }}
            size="small"
            rowKey="id"
            dataSource={item.paymentConfirmation.attachments ?? []}
            pagination={false}
            scroll={{ x: 620 }}
            locale={{ emptyText: 'Chưa có tệp hóa đơn/chứng từ kế toán' }}
            columns={[
              { title: 'File hóa đơn/chứng từ', dataIndex: 'fileName', key: 'fileName' },
              {
                title: 'Dung lượng',
                key: 'fileSize',
                responsive: ['sm'],
                render: (_: unknown, record: Attachment) => formatAttachmentFileSize(record.fileSize),
              },
              {
                title: 'Thao tác',
                key: 'actions',
                render: (_: unknown, record: Attachment) => (
                  <Space wrap className="table-actions">
                    <Button icon={<EyeOutlined />} onClick={() => void openAttachmentPreview(record)} />
                    <Button icon={<DownloadOutlined />} onClick={() => void downloadAttachment(record)} />
                  </Space>
                ),
              },
            ]}
          />
        </Card>
      )}

      <AttachmentPreviewModal
        open={previewState.open}
        fileName={previewState.fileName}
        contentType={previewState.contentType}
        previewUrl={previewState.objectUrl}
        onClose={() => {
          if (previewState.objectUrl) {
            URL.revokeObjectURL(previewState.objectUrl);
          }

          setPreviewState({ open: false });
        }}
      />
    </div>
  );
}
