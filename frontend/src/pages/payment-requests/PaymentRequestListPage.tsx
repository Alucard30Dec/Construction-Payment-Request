import {
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
  SendOutlined,
} from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  DatePicker,
  Input,
  Modal,
  Select,
  Space,
  Table,
  Typography,
  message,
} from 'antd';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { PaymentRequest, PaymentRequestStatus } from '../../types';
import { paymentRequestService } from '../../services/paymentRequestService';
import { useProjectLookup, useSupplierLookup } from '../../hooks/useLookups';
import { paymentRequestStatusOptions } from '../../constants/options';
import { StatusBadge } from '../../components/StatusBadge';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/apiError';

const editableStatuses: PaymentRequestStatus[] = ['Draft', 'ReturnedForEdit', 'Rejected'];
const submittableStatuses: PaymentRequestStatus[] = ['Draft', 'ReturnedForEdit'];

export function PaymentRequestListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const supplierLookup = useSupplierLookup();
  const projectLookup = useProjectLookup();

  const [search, setSearch] = useState('');
  const [projectId, setProjectId] = useState<string | undefined>(undefined);
  const [supplierId, setSupplierId] = useState<string | undefined>(undefined);
  const [status, setStatus] = useState<PaymentRequestStatus | undefined>(undefined);
  const [fromDate, setFromDate] = useState<string | undefined>(undefined);
  const [toDate, setToDate] = useState<string | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const query = useQuery({
    queryKey: [
      'payment-requests',
      search,
      projectId,
      supplierId,
      status,
      fromDate,
      toDate,
      pageNumber,
      pageSize,
    ],
    queryFn: () =>
      paymentRequestService.getPaged({
        search: search || undefined,
        projectId,
        supplierId,
        status,
        fromDate,
        toDate,
        pageNumber,
        pageSize,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: paymentRequestService.remove,
    onSuccess: () => {
      message.success('Đã xóa hồ sơ.');
      void queryClient.invalidateQueries({ queryKey: ['payment-requests'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const submitMutation = useMutation({
    mutationFn: (id: string) => paymentRequestService.submit(id, {}),
    onSuccess: () => {
      message.success('Đã gửi hồ sơ duyệt.');
      void queryClient.invalidateQueries({ queryKey: ['payment-requests'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const columns = useMemo(
    () => [
      { title: 'Mã hồ sơ', dataIndex: 'requestCode', key: 'requestCode' },
      { title: 'Tiêu đề', dataIndex: 'title', key: 'title' },
      { title: 'Dự án', dataIndex: 'projectName', key: 'projectName' },
      { title: 'Nhà cung cấp', dataIndex: 'supplierName', key: 'supplierName' },
      { title: 'Số hóa đơn', dataIndex: 'invoiceNumber', key: 'invoiceNumber' },
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
            <Button icon={<EyeOutlined />} onClick={() => navigate(`/payment-requests/${record.id}`)} />
            {editableStatuses.includes(record.currentStatus) && (
              <Button
                icon={<EditOutlined />}
                onClick={() => navigate(`/payment-requests/${record.id}/edit`)}
              />
            )}
            {submittableStatuses.includes(record.currentStatus) && (
              <Button
                icon={<SendOutlined />}
                onClick={() => {
                  Modal.confirm({
                    title: 'Gửi hồ sơ duyệt',
                    content: `Bạn có chắc muốn gửi hồ sơ ${record.requestCode}?`,
                    onOk: async () => submitMutation.mutateAsync(record.id),
                  });
                }}
              />
            )}
            {editableStatuses.includes(record.currentStatus) && (
              <Button
                danger
                icon={<DeleteOutlined />}
                onClick={() => {
                  Modal.confirm({
                    title: 'Xóa hồ sơ',
                    content: `Bạn có chắc muốn xóa hồ sơ ${record.requestCode}?`,
                    onOk: async () => deleteMutation.mutateAsync(record.id),
                  });
                }}
              />
            )}
          </Space>
        ),
      },
    ],
    [navigate, submitMutation, deleteMutation],
  );

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Hồ sơ thanh toán
        </Typography.Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => navigate('/payment-requests/new')}
        >
          Tạo hồ sơ
        </Button>
      </div>

      <Card>
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo mã hồ sơ, tiêu đề, số hóa đơn"
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
            options={paymentRequestStatusOptions}
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
          <DatePicker
            placeholder="Từ ngày"
            format="DD/MM/YYYY"
            onChange={(value) => {
              setPageNumber(1);
              setFromDate(value ? value.toISOString() : undefined);
            }}
          />
          <DatePicker
            placeholder="Đến ngày"
            format="DD/MM/YYYY"
            onChange={(value) => {
              setPageNumber(1);
              setToDate(
                value ? dayjs(value).hour(23).minute(59).second(59).millisecond(0).toISOString() : undefined,
              );
            }}
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
    </div>
  );
}
