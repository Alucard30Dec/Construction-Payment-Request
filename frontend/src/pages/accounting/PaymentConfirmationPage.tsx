import {
  CheckCircleOutlined,
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
  Modal,
  Select,
  Space,
  Table,
  Typography,
  Upload,
  message,
} from 'antd';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { paymentStatusOptions, paymentRequestStatusOptions } from '../../constants/options';
import { useProjectLookup, useSupplierLookup } from '../../hooks/useLookups';
import {
  paymentRequestService,
  type PaymentConfirmationPayload,
} from '../../services/paymentRequestService';
import type { Attachment, PaymentRequest, PaymentRequestStatus, PaymentStatus } from '../../types';
import { getErrorMessage } from '../../utils/apiError';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../components/StatusBadge';
import { attachmentService } from '../../services/attachmentService';
import { AttachmentPreviewModal } from '../../components/AttachmentPreviewModal';

interface ConfirmationFormValues {
  paymentDate: dayjs.Dayjs;
  paymentReferenceNumber?: string;
  bankTransactionNumber?: string;
  paidAmount: number;
  accountingNote?: string;
  paymentStatus: PaymentStatus;
}

interface ModalState {
  open: boolean;
  record?: PaymentRequest;
}

export function PaymentConfirmationPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [form] = Form.useForm<ConfirmationFormValues>();

  const projectLookup = useProjectLookup();
  const supplierLookup = useSupplierLookup();

  const [search, setSearch] = useState('');
  const [projectId, setProjectId] = useState<string | undefined>(undefined);
  const [supplierId, setSupplierId] = useState<string | undefined>(undefined);
  const [status, setStatus] = useState<PaymentRequestStatus | undefined>('PendingAccounting');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [modalState, setModalState] = useState<ModalState>({ open: false });
  const [previewState, setPreviewState] = useState<{
    open: boolean;
    objectUrl?: string;
    fileName?: string;
    contentType?: string;
  }>({ open: false });

  const query = useQuery({
    queryKey: [
      'payment-requests-confirmation',
      search,
      projectId,
      supplierId,
      status,
      pageNumber,
      pageSize,
    ],
    queryFn: () =>
      paymentRequestService.getPaged({
        search: search || undefined,
        projectId,
        supplierId,
        status,
        pageNumber,
        pageSize,
      }),
  });

  const confirmationAttachmentsQuery = useQuery({
    queryKey: ['payment-confirmation-attachments', modalState.record?.id],
    queryFn: () => attachmentService.getByPaymentConfirmation(modalState.record?.id as string),
    enabled: Boolean(modalState.open && modalState.record?.id),
  });

  const confirmMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: PaymentConfirmationPayload }) =>
      paymentRequestService.confirmPayment(id, payload),
    onSuccess: () => {
      message.success('Xác nhận thanh toán thành công.');
      setModalState({ open: false });
      form.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['payment-requests-confirmation'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-requests'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-confirmation-attachments'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const uploadInvoiceAttachmentMutation = useMutation({
    mutationFn: ({ paymentRequestId, file }: { paymentRequestId: string; file: File }) =>
      attachmentService.uploadPaymentConfirmationAttachment(paymentRequestId, file),
    onSuccess: () => {
      message.success('Đã tải chứng từ/hóa đơn thành công.');
      void queryClient.invalidateQueries({ queryKey: ['payment-confirmation-attachments'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const deleteAttachmentMutation = useMutation({
    mutationFn: attachmentService.delete,
    onSuccess: () => {
      message.success('Đã xóa tệp đính kèm.');
      void queryClient.invalidateQueries({ queryKey: ['payment-confirmation-attachments'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-request-detail'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const openConfirmModal = (record: PaymentRequest) => {
    setModalState({ open: true, record });
    form.setFieldsValue({
      paymentDate: dayjs(),
      paidAmount: record.requestedAmount,
      paymentStatus: 'Paid',
      accountingNote: '',
      paymentReferenceNumber: '',
      bankTransactionNumber: '',
    });
  };

  const columns = useMemo(
    () => [
      { title: 'Mã hồ sơ', dataIndex: 'requestCode', key: 'requestCode' },
      { title: 'Tiêu đề', dataIndex: 'title', key: 'title' },
      { title: 'Dự án', dataIndex: 'projectName', key: 'projectName' },
      { title: 'Nhà cung cấp', dataIndex: 'supplierName', key: 'supplierName' },
      {
        title: 'Hạn thanh toán',
        key: 'dueDate',
        render: (_: unknown, record: PaymentRequest) => formatDate(record.dueDate),
      },
      {
        title: 'Số tiền đề nghị',
        key: 'requestedAmount',
        align: 'right' as const,
        render: (_: unknown, record: PaymentRequest) => formatCurrency(record.requestedAmount),
      },
      {
        title: 'Trạng thái',
        key: 'currentStatus',
        render: (_: unknown, record: PaymentRequest) => <StatusBadge status={record.currentStatus} />,
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: PaymentRequest) => (
          <Space wrap>
            <Button icon={<EyeOutlined />} onClick={() => navigate(`/payment-requests/${record.id}`)}>
              Chi tiết
            </Button>
            <Button
              type="primary"
              icon={<CheckCircleOutlined />}
              disabled={record.currentStatus !== 'PendingAccounting'}
              onClick={() => openConfirmModal(record)}
            >
              Xác nhận
            </Button>
          </Space>
        ),
      },
    ],
    [navigate],
  );

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Kế toán xác nhận thanh toán
        </Typography.Title>
      </div>

      <Card>
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo mã hồ sơ, tiêu đề"
            onSearch={(value) => {
              setPageNumber(1);
              setSearch(value);
            }}
          />
          <Select
            allowClear
            placeholder="Trạng thái"
            value={status}
            onChange={(value) => {
              setPageNumber(1);
              setStatus(value);
            }}
            options={paymentRequestStatusOptions.filter((x) =>
              ['PendingAccounting', 'Paid', 'Rejected', 'ReturnedForEdit'].includes(x.value),
            )}
          />
          <Select
            allowClear
            placeholder="Dự án"
            value={projectId}
            onChange={(value) => {
              setPageNumber(1);
              setProjectId(value);
            }}
            options={(projectLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
          />
          <Select
            allowClear
            placeholder="Nhà cung cấp"
            value={supplierId}
            onChange={(value) => {
              setPageNumber(1);
              setSupplierId(value);
            }}
            options={(supplierLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
          />
        </div>

        <Table<PaymentRequest>
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          pagination={{
            current: pageNumber,
            pageSize,
            total: query.data?.totalCount ?? 0,
            showSizeChanger: true,
            onChange: (page, size) => {
              setPageNumber(page);
              setPageSize(size);
            },
          }}
        />
      </Card>

      <Modal
        open={modalState.open}
        title="Xác nhận thanh toán"
        onCancel={() => {
          setModalState({ open: false });
          form.resetFields();
        }}
        onOk={() => form.submit()}
        confirmLoading={confirmMutation.isPending}
      >
        <Form<ConfirmationFormValues>
          form={form}
          layout="vertical"
          onFinish={(values) => {
            if (!modalState.record) {
              return;
            }

            confirmMutation.mutate({
              id: modalState.record.id,
              payload: {
                paymentDate: values.paymentDate.toISOString(),
                paymentReferenceNumber: values.paymentReferenceNumber,
                bankTransactionNumber: values.bankTransactionNumber,
                paidAmount: values.paidAmount,
                accountingNote: values.accountingNote,
                paymentStatus: values.paymentStatus,
              },
            });
          }}
        >
          <Typography.Paragraph>
            Hồ sơ: <strong>{modalState.record?.requestCode}</strong>
          </Typography.Paragraph>
          <Typography.Paragraph>
            Số tiền đề nghị: <strong>{formatCurrency(modalState.record?.requestedAmount ?? 0)}</strong>
          </Typography.Paragraph>

          <Form.Item
            label="Ngày thanh toán"
            name="paymentDate"
            rules={[{ required: true, message: 'Vui lòng chọn ngày thanh toán.' }]}
          >
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>

          <Form.Item
            label="Số tiền thanh toán"
            name="paidAmount"
            rules={[{ required: true, message: 'Vui lòng nhập số tiền thanh toán.' }]}
          >
            <InputNumber style={{ width: '100%' }} min={0} step={1000000} />
          </Form.Item>

          <Form.Item
            label="Trạng thái thanh toán"
            name="paymentStatus"
            rules={[{ required: true, message: 'Vui lòng chọn trạng thái thanh toán.' }]}
          >
            <Select options={paymentStatusOptions} />
          </Form.Item>

          <Form.Item label="Số tham chiếu thanh toán" name="paymentReferenceNumber">
            <Input />
          </Form.Item>

          <Form.Item label="Mã giao dịch ngân hàng" name="bankTransactionNumber">
            <Input />
          </Form.Item>

          <Form.Item label="Ghi chú kế toán" name="accountingNote">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Divider style={{ margin: '12px 0' }} />
          <Typography.Title level={5}>File hóa đơn/chứng từ kế toán</Typography.Title>
          <Space style={{ marginBottom: 12 }}>
            <Upload
              showUploadList={false}
              beforeUpload={(file) => {
                if (!modalState.record?.id) {
                  return false;
                }

                uploadInvoiceAttachmentMutation.mutate({
                  paymentRequestId: modalState.record.id,
                  file: file as File,
                });
                return false;
              }}
            >
              <Button icon={<UploadOutlined />} loading={uploadInvoiceAttachmentMutation.isPending}>
                Tải file hóa đơn/chứng từ
              </Button>
            </Upload>
          </Space>

          <Table<Attachment>
            rowKey="id"
            size="small"
            loading={confirmationAttachmentsQuery.isLoading}
            dataSource={confirmationAttachmentsQuery.data ?? []}
            pagination={false}
            locale={{ emptyText: 'Chưa có tệp đính kèm' }}
            columns={[
              { title: 'Tên tệp', dataIndex: 'fileName', key: 'fileName' },
              {
                title: 'Dung lượng',
                key: 'fileSize',
                render: (_, record) => `${Math.round(record.fileSize / 1024)} KB`,
              },
              {
                title: 'Thao tác',
                key: 'actions',
                render: (_, record) => (
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
                    <Button
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => deleteAttachmentMutation.mutate(record.id)}
                    />
                  </Space>
                ),
              },
            ]}
          />
        </Form>
      </Modal>

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
