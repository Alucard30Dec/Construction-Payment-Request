import {
  DeleteOutlined,
  DownloadOutlined,
  EyeOutlined,
  UploadOutlined,
} from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  DatePicker,
  Divider,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Table,
  Typography,
  Upload,
  message,
} from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { Attachment, PaymentRequestApprovalHistory } from '../../types';
import type { PaymentRequestPayload } from '../../services/paymentRequestService';
import { paymentRequestService } from '../../services/paymentRequestService';
import { attachmentService } from '../../services/attachmentService';
import { paymentMethodOptions } from '../../constants/options';
import { useContractLookup, useProjectLookup, useSupplierLookup } from '../../hooks/useLookups';
import { formatCurrency, formatDateTime } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/apiError';
import { AttachmentPreviewModal } from '../../components/AttachmentPreviewModal';
import { AttachmentPreviewGallery } from '../../components/AttachmentPreviewGallery';
import {
  ATTACHMENT_ACCEPT,
  formatAttachmentFileSize,
  isPreviewableAttachment,
  validateAttachmentFile,
} from '../../utils/attachmentFiles';

interface PaymentRequestFormValues {
  requestCode: string;
  title: string;
  projectId: string;
  supplierId: string;
  contractId?: string;
  requestType: string;
  invoiceNumber: string;
  invoiceDate: dayjs.Dayjs;
  dueDate: dayjs.Dayjs;
  description?: string;
  amountBeforeVat: number;
  vatRate: number;
  advanceDeduction: number;
  retentionAmount: number;
  otherDeduction: number;
  paymentMethod: 'BankTransfer' | 'Cash' | 'Other';
  notes?: string;
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

export function PaymentRequestFormPage() {
  const [form] = Form.useForm<PaymentRequestFormValues>();
  const navigate = useNavigate();
  const params = useParams<{ id: string }>();
  const id = params.id;
  const queryClient = useQueryClient();

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
  const selectedProjectId = Form.useWatch('projectId', form);
  const selectedSupplierId = Form.useWatch('supplierId', form);
  const selectedContractId = Form.useWatch('contractId', form);
  const contractLookup = useContractLookup({
    projectId: selectedProjectId,
    supplierId: selectedSupplierId,
  });

  const detailQuery = useQuery({
    queryKey: ['payment-request-detail', id],
    queryFn: () => paymentRequestService.getById(id as string),
    enabled: Boolean(id),
  });

  const attachmentsQuery = useQuery({
    queryKey: ['attachments', id],
    queryFn: () => attachmentService.getByPaymentRequestId(id as string),
    enabled: Boolean(id),
  });

  useEffect(() => {
    if (!detailQuery.data) {
      return;
    }

    form.setFieldsValue({
      ...detailQuery.data,
      invoiceDate: dayjs(detailQuery.data.invoiceDate),
      dueDate: dayjs(detailQuery.data.dueDate),
    });
  }, [detailQuery.data, form]);

  useEffect(() => {
    if (!selectedContractId) {
      return;
    }

    if (!selectedProjectId) {
      form.setFieldValue('contractId', undefined);
      return;
    }

    if (contractLookup.isFetching) {
      return;
    }

    const isSelectedContractValid = (contractLookup.data ?? []).some((contract) => contract.id === selectedContractId);
    if (!isSelectedContractValid) {
      form.setFieldValue('contractId', undefined);
    }
  }, [contractLookup.data, contractLookup.isFetching, form, selectedContractId, selectedProjectId]);

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
    mutationFn: async (values: PaymentRequestFormValues) => {
      const payload: PaymentRequestPayload = {
        requestCode: values.requestCode,
        title: values.title,
        projectId: values.projectId,
        supplierId: values.supplierId,
        contractId: values.contractId,
        requestType: values.requestType,
        invoiceNumber: values.invoiceNumber,
        invoiceDate: values.invoiceDate.toISOString(),
        dueDate: values.dueDate.toISOString(),
        description: values.description,
        amountBeforeVat: values.amountBeforeVat,
        vatRate: values.vatRate,
        advanceDeduction: values.advanceDeduction,
        retentionAmount: values.retentionAmount,
        otherDeduction: values.otherDeduction,
        paymentMethod: values.paymentMethod,
        notes: values.notes,
      };

      if (id) {
        return paymentRequestService.update(id, payload);
      }

      return paymentRequestService.create(payload);
    },
    onSuccess: async (savedPaymentRequest) => {
      let uploadedCount = 0;
      let failedCount = 0;
      const failedMessages: string[] = [];

      if (!id && pendingAttachments.length > 0) {
        setIsUploadingPending(true);

        for (const pendingAttachment of pendingAttachments) {
          try {
            await attachmentService.uploadPaymentRequestAttachment(savedPaymentRequest.id, pendingAttachment.file);
            uploadedCount += 1;
          } catch (error) {
            failedCount += 1;
            failedMessages.push(`${pendingAttachment.fileName}: ${getErrorMessage(error)}`);
          }
        }

        setIsUploadingPending(false);
      }

      void queryClient.invalidateQueries({ queryKey: ['payment-requests'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail', savedPaymentRequest.id] });
      void queryClient.invalidateQueries({ queryKey: ['attachments', savedPaymentRequest.id] });

      if (id) {
        message.success('Cập nhật hồ sơ thành công.');
      } else if (pendingAttachments.length === 0) {
        message.success('Tạo hồ sơ thành công.');
      } else if (failedCount === 0) {
        message.success(`Tạo hồ sơ và tải lên ${uploadedCount} tệp thành công.`);
        clearPendingAttachments();
      } else {
        message.error(
          `Tạo hồ sơ thành công nhưng chỉ tải ${uploadedCount}/${pendingAttachments.length} tệp. ` +
            `Lỗi: ${failedMessages.join(' | ')}`,
        );
        navigate(`/payment-requests/${savedPaymentRequest.id}/edit`);
        return;
      }

      navigate(`/payment-requests/${savedPaymentRequest.id}`);
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const uploadMutation = useMutation({
    mutationFn: (file: File) => attachmentService.uploadPaymentRequestAttachment(id as string, file),
    onSuccess: () => {
      message.success('Tải tệp lên thành công.');
      void queryClient.invalidateQueries({ queryKey: ['attachments', id] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail', id] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const deleteAttachmentMutation = useMutation({
    mutationFn: attachmentService.delete,
    onSuccess: () => {
      message.success('Đã xóa tệp.');
      void queryClient.invalidateQueries({ queryKey: ['attachments', id] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail', id] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const amountBeforeVat = Form.useWatch('amountBeforeVat', form) ?? 0;
  const vatRate = Form.useWatch('vatRate', form) ?? 0;
  const advanceDeduction = Form.useWatch('advanceDeduction', form) ?? 0;
  const retentionAmount = Form.useWatch('retentionAmount', form) ?? 0;
  const otherDeduction = Form.useWatch('otherDeduction', form) ?? 0;

  const { vatAmount, amountAfterVat, requestedAmount } = useMemo(() => {
    const vat = (amountBeforeVat * vatRate) / 100;
    const afterVat = amountBeforeVat + vat;
    const requested = afterVat - advanceDeduction - retentionAmount - otherDeduction;

    return {
      vatAmount: vat,
      amountAfterVat: afterVat,
      requestedAmount: requested,
    };
  }, [amountBeforeVat, vatRate, advanceDeduction, retentionAmount, otherDeduction]);

  const approvalColumns = useMemo(
    () => [
      {
        title: 'Bước',
        dataIndex: 'stepOrder',
        key: 'stepOrder',
      },
      {
        title: 'Người xử lý',
        key: 'approver',
        render: (_: unknown, record: PaymentRequestApprovalHistory) =>
          `${record.approverFullName} (${record.approverUsername})`,
      },
      {
        title: 'Hành động',
        dataIndex: 'action',
        key: 'action',
      },
      {
        title: 'Thời gian',
        key: 'actionAt',
        render: (_: unknown, record: PaymentRequestApprovalHistory) => formatDateTime(record.actionAt),
      },
      {
        title: 'Ghi chú',
        dataIndex: 'comment',
        key: 'comment',
      },
    ],
    [],
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

  const isSaving = saveMutation.isPending || isUploadingPending;
  const contractOptions = useMemo(
    () =>
      (contractLookup.data ?? []).map((x) => ({
        label: `${x.contractNumber} - ${x.name}`,
        value: x.id,
      })),
    [contractLookup.data],
  );
  const firstPendingPreview = useMemo(
    () => pendingAttachments.find((item) => Boolean(item.previewUrl)),
    [pendingAttachments],
  );

  return (
    <div className="page-stack">
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          {id ? 'Sửa hồ sơ thanh toán' : 'Tạo hồ sơ thanh toán'}
        </Typography.Title>
      </div>

      <Card className="page-card" loading={detailQuery.isLoading && Boolean(id)}>
        <Form<PaymentRequestFormValues>
          form={form}
          layout="vertical"
          initialValues={{
            paymentMethod: 'BankTransfer',
            vatRate: 10,
            amountBeforeVat: 0,
            advanceDeduction: 0,
            retentionAmount: 0,
            otherDeduction: 0,
          }}
          onFinish={(values) => saveMutation.mutate(values)}
        >
          <Typography.Title level={5}>Thông tin chung</Typography.Title>
          <div className="form-grid form-grid--wide">
            <Form.Item
              label="Mã hồ sơ"
              name="requestCode"
              rules={[{ required: true, message: 'Vui lòng nhập mã hồ sơ.' }]}
            >
              <Input disabled={Boolean(id)} />
            </Form.Item>
            <Form.Item
              label="Tiêu đề"
              name="title"
              rules={[{ required: true, message: 'Vui lòng nhập tiêu đề.' }]}
            >
              <Input />
            </Form.Item>
            <Form.Item label="Dự án" name="projectId" rules={[{ required: true }]}> 
              <Select
                options={(projectLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Nhà cung cấp" name="supplierId" rules={[{ required: true }]}> 
              <Select
                options={(supplierLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Hợp đồng" name="contractId">
              <Select
                allowClear
                disabled={!selectedProjectId}
                loading={contractLookup.isFetching}
                options={contractOptions}
                placeholder={selectedProjectId ? 'Chá»n há»£p Ä‘á»“ng' : 'Chá»n dá»± Ã¡n trÆ°á»›c'}
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Loại đề nghị" name="requestType" rules={[{ required: true }]}> 
              <Input />
            </Form.Item>
            <Form.Item label="Phương thức thanh toán" name="paymentMethod" rules={[{ required: true }]}> 
              <Select options={paymentMethodOptions} />
            </Form.Item>
          </div>

          <Divider />
          <Typography.Title level={5}>Thông tin hóa đơn</Typography.Title>
          <div className="form-grid">
            <Form.Item label="Số hóa đơn" name="invoiceNumber" rules={[{ required: true }]}> 
              <Input />
            </Form.Item>
            <Form.Item label="Ngày hóa đơn" name="invoiceDate" rules={[{ required: true }]}> 
              <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
            </Form.Item>
            <Form.Item label="Hạn thanh toán" name="dueDate" rules={[{ required: true }]}> 
              <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
            </Form.Item>
          </div>

          <Form.Item label="Mô tả" name="description">
            <Input.TextArea rows={2} />
          </Form.Item>

          <Divider />
          <Typography.Title level={5}>Giá trị thanh toán</Typography.Title>
          <div className="form-grid form-grid--wide">
            <Form.Item label="Trước VAT" name="amountBeforeVat" rules={[{ required: true }]}> 
              <InputNumber style={{ width: '100%' }} min={0} />
            </Form.Item>
            <Form.Item label="VAT (%)" name="vatRate" rules={[{ required: true }]}> 
              <InputNumber style={{ width: '100%' }} min={0} max={100} />
            </Form.Item>
            <Form.Item label="Khấu trừ tạm ứng" name="advanceDeduction">
              <InputNumber style={{ width: '100%' }} min={0} />
            </Form.Item>
            <Form.Item label="Giữ lại bảo hành" name="retentionAmount">
              <InputNumber style={{ width: '100%' }} min={0} />
            </Form.Item>
            <Form.Item label="Khấu trừ khác" name="otherDeduction">
              <InputNumber style={{ width: '100%' }} min={0} />
            </Form.Item>
          </div>

          <Card size="small" style={{ marginBottom: 16, background: '#fafafa' }}>
            <Space direction="vertical" size={4}>
              <Typography.Text>VAT: {formatCurrency(vatAmount)}</Typography.Text>
              <Typography.Text>Sau VAT: {formatCurrency(amountAfterVat)}</Typography.Text>
              <Typography.Text strong>Đề nghị thanh toán: {formatCurrency(requestedAmount)}</Typography.Text>
            </Space>
          </Card>

          <Form.Item label="Ghi chú" name="notes">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Space>
            <Button onClick={() => navigate('/payment-requests')}>Hủy</Button>
            <Button type="primary" htmlType="submit" loading={isSaving}>
              {isUploadingPending ? 'Đang tải tệp...' : 'Lưu hồ sơ'}
            </Button>
          </Space>
        </Form>
      </Card>

      <Card
        title="File đính kèm"
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
              emptyText="Chưa có tệp hồ sơ để xem trước."
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
            <Typography.Text type="secondary">Chưa có tệp hồ sơ để xem trước.</Typography.Text>
          )}
        </Card>

        {id ? (
          <Table<Attachment>
            rowKey="id"
            size="small"
            dataSource={attachmentsQuery.data ?? []}
            pagination={false}
            locale={{ emptyText: 'Chưa có tệp đính kèm' }}
            columns={serverColumns}
          />
        ) : (
          <Table<PendingAttachment>
            rowKey="id"
            size="small"
            dataSource={pendingAttachments}
            pagination={false}
            locale={{ emptyText: 'Chưa có tệp. Tệp sẽ được tải lên ngay sau khi lưu hồ sơ.' }}
            columns={pendingColumns}
          />
        )}
      </Card>

      {id && detailQuery.data?.approvalHistories && detailQuery.data.approvalHistories.length > 0 && (
        <Card title="Lịch sử duyệt">
          <Table<PaymentRequestApprovalHistory>
            rowKey="id"
            size="small"
            dataSource={detailQuery.data.approvalHistories}
            columns={approvalColumns}
            pagination={false}
          />
        </Card>
      )}

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
