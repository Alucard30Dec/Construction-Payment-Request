import { DeleteOutlined, DownloadOutlined, EyeOutlined, UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  DatePicker,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Switch,
  Table,
  Typography,
  Upload,
  message,
} from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { contractService, type ContractPayload } from '../../services/contractService';
import { contractTypeOptions } from '../../constants/options';
import { useProjectLookup, useSupplierLookup } from '../../hooks/useLookups';
import { getErrorMessage } from '../../utils/apiError';
import { attachmentService } from '../../services/attachmentService';
import type { Attachment } from '../../types';
import { AttachmentPreviewModal } from '../../components/AttachmentPreviewModal';
import { AttachmentPreviewGallery } from '../../components/AttachmentPreviewGallery';
import {
  ATTACHMENT_ACCEPT,
  formatAttachmentFileSize,
  isPreviewableAttachment,
  validateAttachmentFile,
} from '../../utils/attachmentFiles';

interface ContractFormValues extends Omit<ContractPayload, 'signedDate'> {
  signedDate: dayjs.Dayjs;
}

interface PendingAttachment {
  id: string;
  file: File;
  fileName: string;
  fileSize: number;
  contentType: string;
  createdAt: string;
  previewUrl?: string;
}

function downloadLocalFile(file: File): void {
  const objectUrl = URL.createObjectURL(file);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = file.name;
  anchor.style.display = 'none';

  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
}

export function ContractFormPage() {
  const [form] = Form.useForm<ContractFormValues>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const params = useParams<{ id: string }>();
  const id = params.id;
  const [pendingAttachments, setPendingAttachments] = useState<PendingAttachment[]>([]);
  const [isUploadingPending, setIsUploadingPending] = useState(false);
  const pendingPreviewUrlsRef = useRef<Set<string>>(new Set());
  const [previewState, setPreviewState] = useState<{
    open: boolean;
    objectUrl?: string;
    fileName?: string;
    contentType?: string;
    revokeOnClose?: boolean;
  }>({ open: false });

  const supplierLookup = useSupplierLookup();
  const projectLookup = useProjectLookup();

  const detailQuery = useQuery({
    queryKey: ['contract-detail', id],
    queryFn: () => contractService.getById(id as string),
    enabled: Boolean(id),
  });

  const attachmentsQuery = useQuery({
    queryKey: ['contract-attachments', id],
    queryFn: () => attachmentService.getByContractId(id as string),
    enabled: Boolean(id),
  });

  useEffect(() => {
    if (detailQuery.data) {
      form.setFieldsValue({
        ...detailQuery.data,
        signedDate: dayjs(detailQuery.data.signedDate),
      });
    }
  }, [detailQuery.data, form]);

  useEffect(
    () => () => {
      pendingPreviewUrlsRef.current.forEach((url) => URL.revokeObjectURL(url));
      pendingPreviewUrlsRef.current.clear();
    },
    [],
  );

  const revokePendingPreviewUrl = (previewUrl?: string): void => {
    if (!previewUrl) {
      return;
    }

    if (pendingPreviewUrlsRef.current.has(previewUrl)) {
      URL.revokeObjectURL(previewUrl);
      pendingPreviewUrlsRef.current.delete(previewUrl);
    }
  };

  const removePendingAttachment = (pendingId: string): void => {
    setPendingAttachments((prev) => {
      const target = prev.find((item) => item.id === pendingId);
      if (target) {
        revokePendingPreviewUrl(target.previewUrl);
      }

      return prev.filter((item) => item.id !== pendingId);
    });
  };

  const clearPendingAttachments = (): void => {
    setPendingAttachments((prev) => {
      prev.forEach((item) => revokePendingPreviewUrl(item.previewUrl));
      return [];
    });
  };

  const addPendingAttachment = (file: File): void => {
    const validationError = validateAttachmentFile(file);
    if (validationError) {
      message.error(validationError);
      return;
    }

    setPendingAttachments((prev) => {
      const duplicated = prev.some((item) => item.fileName === file.name && item.fileSize === file.size);
      if (duplicated) {
        message.warning(`Tệp ${file.name} đã có trong danh sách chờ.`);
        return prev;
      }

      const shouldCreatePreviewUrl = isPreviewableAttachment(file.type, file.name);
      const previewUrl = shouldCreatePreviewUrl ? URL.createObjectURL(file) : undefined;
      if (previewUrl) {
        pendingPreviewUrlsRef.current.add(previewUrl);
      }

      return [
        ...prev,
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          file,
          fileName: file.name,
          fileSize: file.size,
          contentType: file.type,
          createdAt: new Date().toISOString(),
          previewUrl,
        },
      ];
    });
  };

  const saveMutation = useMutation({
    mutationFn: async (values: ContractFormValues) => {
      const payload: ContractPayload = {
        ...values,
        signedDate: values.signedDate.toISOString(),
      };

      if (id) {
        return contractService.update(id, payload);
      }

      return contractService.create(payload);
    },
    onSuccess: async (savedContract) => {
      let uploadedCount = 0;
      let failedCount = 0;
      const failedMessages: string[] = [];

      if (!id && pendingAttachments.length > 0) {
        setIsUploadingPending(true);

        for (const pendingAttachment of pendingAttachments) {
          try {
            await attachmentService.uploadContractAttachment(savedContract.id, pendingAttachment.file);
            uploadedCount += 1;
          } catch (error) {
            failedCount += 1;
            failedMessages.push(`${pendingAttachment.fileName}: ${getErrorMessage(error)}`);
          }
        }

        setIsUploadingPending(false);
      }

      void queryClient.invalidateQueries({ queryKey: ['contracts'] });
      void queryClient.invalidateQueries({ queryKey: ['contract-detail', savedContract.id] });
      void queryClient.invalidateQueries({ queryKey: ['contract-attachments', savedContract.id] });

      if (id) {
        message.success('Cập nhật hợp đồng thành công.');
      } else if (pendingAttachments.length === 0) {
        message.success('Tạo hợp đồng thành công.');
      } else if (failedCount === 0) {
        message.success(`Tạo hợp đồng và tải lên ${uploadedCount} tệp thành công.`);
        clearPendingAttachments();
      } else {
        message.error(
          `Tạo hợp đồng thành công nhưng chỉ tải ${uploadedCount}/${pendingAttachments.length} tệp. ` +
            `Lỗi: ${failedMessages.join(' | ')}`,
        );
        navigate(`/contracts/${savedContract.id}/edit`);
        return;
      }

      navigate(`/contracts/${savedContract.id}`);
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const uploadMutation = useMutation({
    mutationFn: (file: File) => attachmentService.uploadContractAttachment(id as string, file),
    onSuccess: () => {
      message.success('Tải tệp hợp đồng thành công.');
      void queryClient.invalidateQueries({ queryKey: ['contract-attachments', id] });
      void queryClient.invalidateQueries({ queryKey: ['contract-detail', id] });
      void queryClient.invalidateQueries({ queryKey: ['contracts'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const deleteAttachmentMutation = useMutation({
    mutationFn: attachmentService.delete,
    onSuccess: () => {
      message.success('Đã xóa tệp hợp đồng.');
      void queryClient.invalidateQueries({ queryKey: ['contract-attachments', id] });
      void queryClient.invalidateQueries({ queryKey: ['contract-detail', id] });
      void queryClient.invalidateQueries({ queryKey: ['contracts'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const isSaving = saveMutation.isPending || isUploadingPending;
  const firstPendingPreview = useMemo(
    () => pendingAttachments.find((item) => Boolean(item.previewUrl)),
    [pendingAttachments],
  );

  const pendingColumns = useMemo(
    () => [
      { title: 'Tên tệp', dataIndex: 'fileName', key: 'fileName' },
      {
        title: 'Dung lượng',
        key: 'fileSize',
        render: (_: unknown, record: PendingAttachment) => formatAttachmentFileSize(record.fileSize),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: PendingAttachment) => (
          <Space>
            <Button
              icon={<EyeOutlined />}
              onClick={() => {
                if (!record.previewUrl) {
                  message.info('Định dạng này chưa hỗ trợ preview trực tiếp. Vui lòng tải xuống để kiểm tra.');
                  return;
                }

                setPreviewState({
                  open: true,
                  objectUrl: record.previewUrl,
                  fileName: record.fileName,
                  contentType: record.contentType,
                  revokeOnClose: false,
                });
              }}
            />
            <Button icon={<DownloadOutlined />} onClick={() => downloadLocalFile(record.file)} />
            <Button danger icon={<DeleteOutlined />} onClick={() => removePendingAttachment(record.id)} />
          </Space>
        ),
      },
    ],
    [],
  );

  const serverColumns = useMemo(
    () => [
      { title: 'Tên tệp', dataIndex: 'fileName', key: 'fileName' },
      {
        title: 'Dung lượng',
        key: 'fileSize',
        render: (_: unknown, record: Attachment) => formatAttachmentFileSize(record.fileSize),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Attachment) => (
          <Space>
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
                    revokeOnClose: true,
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
            <Button danger icon={<DeleteOutlined />} onClick={() => deleteAttachmentMutation.mutate(record.id)} />
          </Space>
        ),
      },
    ],
    [deleteAttachmentMutation],
  );

  return (
    <div className="page-stack">
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          {id ? 'Sửa hợp đồng' : 'Thêm hợp đồng'}
        </Typography.Title>
      </div>

      <Card className="page-card" loading={detailQuery.isLoading}>
        <Form<ContractFormValues>
          form={form}
          layout="vertical"
          initialValues={{ isActive: true, contractType: 'Construction' }}
          onFinish={(values) => saveMutation.mutate(values)}
        >
          <div className="form-grid form-grid--wide">
            <Form.Item label="Số hợp đồng" name="contractNumber" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Tên hợp đồng" name="name" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Nhà cung cấp" name="supplierId" rules={[{ required: true }]}> 
              <Select
                options={(supplierLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Dự án" name="projectId" rules={[{ required: true }]}> 
              <Select
                options={(projectLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Ngày ký" name="signedDate" rules={[{ required: true }]}> 
              <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
            </Form.Item>
            <Form.Item label="Loại hợp đồng" name="contractType" rules={[{ required: true }]}> 
              <Select options={contractTypeOptions} />
            </Form.Item>
            <Form.Item label="Giá trị hợp đồng" name="contractValue" rules={[{ required: true }]}> 
              <InputNumber style={{ width: '100%' }} min={0} step={1000000} />
            </Form.Item>
          </div>

          <Form.Item label="Ghi chú" name="notes">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>

          <Space>
            <Button onClick={() => navigate('/contracts')}>Hủy</Button>
            <Button type="primary" htmlType="submit" loading={isSaving}>
              {isUploadingPending ? 'Đang tải tệp...' : 'Lưu'}
            </Button>
          </Space>
        </Form>
      </Card>

      <Card
        title="Tệp hợp đồng"
        loading={Boolean(id) && attachmentsQuery.isLoading}
        extra={
          <Typography.Text type="secondary">Định dạng: PDF/DOC/DOCX/XLS/XLSX - Tối đa 20 MB/tệp</Typography.Text>
        }
      >
        <Space style={{ marginBottom: 12 }}>
          <Upload
            showUploadList={false}
            accept={ATTACHMENT_ACCEPT}
            beforeUpload={(file) => {
              if (id) {
                const validationError = validateAttachmentFile(file as File);
                if (validationError) {
                  message.error(validationError);
                  return false;
                }

                uploadMutation.mutate(file as File);
                return false;
              }

              addPendingAttachment(file as File);
              return false;
            }}
          >
            <Button icon={<UploadOutlined />} loading={uploadMutation.isPending}>
              {id ? 'Tải tệp lên' : 'Thêm tệp chờ upload'}
            </Button>
          </Upload>
        </Space>

        <Card
          size="small"
          title="Xem trước nhanh (1 tệp)"
          style={{ marginBottom: 12, background: '#fafcff' }}
        >
          {id ? (
            <AttachmentPreviewGallery
              attachments={attachmentsQuery.data ?? []}
              maxItems={1}
              emptyText="Chưa có tệp hợp đồng để xem trước."
            />
          ) : firstPendingPreview?.previewUrl ? (
            <div style={{ display: 'grid', gap: 8 }}>
              <div
                style={{
                  border: '1px solid #dbe4ee',
                  borderRadius: 8,
                  minHeight: 220,
                  overflow: 'hidden',
                  background: '#f8fafc',
                }}
              >
                {firstPendingPreview.contentType?.toLowerCase().startsWith('image/') ? (
                  <img
                    src={firstPendingPreview.previewUrl}
                    alt={firstPendingPreview.fileName}
                    style={{ width: '100%', maxHeight: 300, objectFit: 'contain' }}
                  />
                ) : (
                  <iframe
                    src={firstPendingPreview.previewUrl}
                    title={firstPendingPreview.fileName}
                    style={{ width: '100%', height: 300, border: 0 }}
                  />
                )}
              </div>
              <Typography.Text strong>{firstPendingPreview.fileName}</Typography.Text>
              <Typography.Text type="secondary">
                {formatAttachmentFileSize(firstPendingPreview.fileSize)}
              </Typography.Text>
            </div>
          ) : pendingAttachments.length > 0 ? (
            <Typography.Text type="secondary">
              Tệp vừa chọn không hỗ trợ preview trực tiếp. Bạn vẫn có thể tải xuống để kiểm tra.
            </Typography.Text>
          ) : (
            <Typography.Text type="secondary">Chưa có tệp hợp đồng để xem trước.</Typography.Text>
          )}
        </Card>

        {id ? (
          <Table<Attachment>
            rowKey="id"
            size="small"
            dataSource={attachmentsQuery.data ?? []}
            pagination={false}
            locale={{ emptyText: 'Chưa có tệp hợp đồng' }}
            columns={serverColumns}
          />
        ) : (
          <Table<PendingAttachment>
            rowKey="id"
            size="small"
            dataSource={pendingAttachments}
            pagination={false}
            locale={{ emptyText: 'Chưa có tệp. Tệp sẽ được tải lên ngay sau khi lưu hợp đồng.' }}
            columns={pendingColumns}
          />
        )}
      </Card>

      <AttachmentPreviewModal
        open={previewState.open}
        fileName={previewState.fileName}
        contentType={previewState.contentType}
        previewUrl={previewState.objectUrl}
        onClose={() => {
          if (previewState.revokeOnClose && previewState.objectUrl) {
            URL.revokeObjectURL(previewState.objectUrl);
          }

          setPreviewState({ open: false });
        }}
      />
    </div>
  );
}
