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
import type { Attachment } from '../../types';
import { contractService } from '../../services/contractService';
import { attachmentService } from '../../services/attachmentService';
import { formatAttachmentFileSize } from '../../utils/attachmentFiles';
import { formatCurrency, formatDate, formatDateTime } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/apiError';
import { AttachmentPreviewGallery } from '../../components/AttachmentPreviewGallery';
import { AttachmentPreviewModal } from '../../components/AttachmentPreviewModal';

export function ContractDetailPage() {
  const params = useParams<{ id: string }>();
  const navigate = useNavigate();
  const id = params.id as string;
  const [previewState, setPreviewState] = useState<{
    open: boolean;
    objectUrl?: string;
    fileName?: string;
    contentType?: string;
  }>({ open: false });

  const contractQuery = useQuery({
    queryKey: ['contract-detail', id],
    queryFn: () => contractService.getById(id),
  });

  const contract = contractQuery.data;

  return (
    <div className="page-stack">
      <div className="page-header">
        <div className="page-header__content">
          <Typography.Title level={3} style={{ margin: 0 }}>
            Chi tiết hợp đồng
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Trang chi tiết được dàn lại bằng mô tả responsive để không bị vỡ bố cục trên điện thoại.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Space wrap>
            <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/contracts')}>
              Quay lại
            </Button>
            {contract && (
              <Button
                type="primary"
                icon={<EditOutlined />}
                onClick={() => navigate(`/contracts/${contract.id}/edit`)}
              >
                Sửa hợp đồng
              </Button>
            )}
          </Space>
        </div>
      </div>

      <Card className="page-card descriptions-responsive" loading={contractQuery.isLoading}>
        {contract && (
          <Descriptions bordered size="small" column={{ xs: 1, sm: 1, md: 2 }}>
            <Descriptions.Item label="Số hợp đồng">{contract.contractNumber}</Descriptions.Item>
            <Descriptions.Item label="Loại hợp đồng">{contract.contractType}</Descriptions.Item>
            <Descriptions.Item label="Tên hợp đồng" span={2}>
              {contract.name}
            </Descriptions.Item>
            <Descriptions.Item label="Nhà cung cấp">{contract.supplierName}</Descriptions.Item>
            <Descriptions.Item label="Dự án">{contract.projectName}</Descriptions.Item>
            <Descriptions.Item label="Ngày ký">{formatDate(contract.signedDate)}</Descriptions.Item>
            <Descriptions.Item label="Giá trị hợp đồng">
              <strong>{formatCurrency(contract.contractValue)}</strong>
            </Descriptions.Item>
            <Descriptions.Item label="Trạng thái">
              {contract.isActive ? 'Hoạt động' : 'Ngưng hoạt động'}
            </Descriptions.Item>
            <Descriptions.Item label="Ngày cập nhật">{formatDateTime(contract.updatedAt)}</Descriptions.Item>
            <Descriptions.Item label="Ghi chú" span={2}>
              {contract.notes ?? '-'}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Card>

      <Card className="page-card" title="Bản xem trước tệp hợp đồng" loading={contractQuery.isLoading}>
        <AttachmentPreviewGallery
          attachments={contract?.attachments ?? []}
          emptyText="Chưa có tệp hợp đồng để xem trước."
        />
      </Card>

      <Card className="page-card" title="Danh sách tệp hợp đồng" loading={contractQuery.isLoading}>
        <Table<Attachment>
          className="responsive-table"
          rowKey="id"
          size="small"
          dataSource={contract?.attachments ?? []}
          pagination={false}
          scroll={{ x: 620 }}
          locale={{ emptyText: 'Chưa có tệp hợp đồng' }}
          columns={[
            { title: 'Tên tệp', dataIndex: 'fileName', key: 'fileName' },
            {
              title: 'Dung lượng',
              key: 'fileSize',
              responsive: ['sm'],
              render: (_: unknown, record) => formatAttachmentFileSize(record.fileSize),
            },
            {
              title: 'Thao tác',
              key: 'actions',
              render: (_: unknown, record) => (
                <Space wrap className="table-actions">
                  <Button
                    icon={<EyeOutlined />}
                    onClick={async () => {
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
                    }}
                  />
                  <Button
                    icon={<DownloadOutlined />}
                    onClick={async () => {
                      try {
                        await attachmentService.download(record.id, record.fileName);
                      } catch (error) {
                        message.error(getErrorMessage(error));
                      }
                    }}
                  />
                </Space>
              ),
            },
          ]}
        />
      </Card>

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
