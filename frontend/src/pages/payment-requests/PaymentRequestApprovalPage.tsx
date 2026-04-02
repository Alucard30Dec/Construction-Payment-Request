import { CheckOutlined, CloseOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Form, Input, Modal, Select, Space, Table, Typography, message } from 'antd';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { PaymentRequest, PaymentRequestStatus } from '../../types';
import { paymentRequestService } from '../../services/paymentRequestService';
import { StatusBadge } from '../../components/StatusBadge';
import { paymentRequestStatusOptions } from '../../constants/options';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/apiError';
import { useAuth } from '../../hooks/useAuth';

interface ActionModalState {
  open: boolean;
  action: 'approve' | 'reject' | 'return';
  record?: PaymentRequest;
}

function canApprove(role: string | undefined, status: PaymentRequestStatus): boolean {
  if (!role) {
    return false;
  }

  if (status === 'PendingDepartmentApproval') {
    return role === 'Admin' || role === 'DepartmentManager';
  }

  if (status === 'PendingDirectorApproval') {
    return role === 'Admin' || role === 'Director';
  }

  return false;
}

function canRejectOrReturn(role: string | undefined, status: PaymentRequestStatus): boolean {
  if (!role) {
    return false;
  }

  if (status === 'PendingDepartmentApproval') {
    return role === 'Admin' || role === 'DepartmentManager';
  }

  if (status === 'PendingDirectorApproval') {
    return role === 'Admin' || role === 'Director';
  }

  if (status === 'PendingAccounting') {
    return role === 'Admin' || role === 'Accountant';
  }

  return false;
}

function getDefaultStatusByRole(role?: string): PaymentRequestStatus | undefined {
  if (role === 'DepartmentManager') {
    return 'PendingDepartmentApproval';
  }

  if (role === 'Director') {
    return 'PendingDirectorApproval';
  }

  if (role === 'Accountant') {
    return 'PendingAccounting';
  }

  return undefined;
}

export function PaymentRequestApprovalPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [form] = Form.useForm<{ comment?: string }>();

  const [status, setStatus] = useState<PaymentRequestStatus | undefined>(
    getDefaultStatusByRole(user?.role),
  );
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [modal, setModal] = useState<ActionModalState>({ open: false, action: 'approve' });

  const query = useQuery({
    queryKey: ['payment-requests-approvals', status, pageNumber, pageSize],
    queryFn: () =>
      paymentRequestService.getPaged({
        status,
        pageNumber,
        pageSize,
      }),
  });

  const actionMutation = useMutation({
    mutationFn: async ({
      action,
      id,
      comment,
    }: {
      action: 'approve' | 'reject' | 'return';
      id: string;
      comment?: string;
    }) => {
      if (action === 'approve') {
        return paymentRequestService.approve(id, { comment });
      }

      if (action === 'reject') {
        return paymentRequestService.reject(id, { comment });
      }

      return paymentRequestService.returnForEdit(id, { comment });
    },
    onSuccess: () => {
      message.success('Thao tác thành công.');
      setModal({ open: false, action: 'approve' });
      form.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['payment-requests-approvals'] });
      void queryClient.invalidateQueries({ queryKey: ['payment-requests'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const columns = useMemo(
    () => [
      { title: 'Mã hồ sơ', dataIndex: 'requestCode', key: 'requestCode', responsive: ['sm'] as Array<'sm'> },
      { title: 'Tiêu đề', dataIndex: 'title', key: 'title' },
      { title: 'Dự án', dataIndex: 'projectName', key: 'projectName', responsive: ['lg'] as Array<'lg'> },
      { title: 'Nhà cung cấp', dataIndex: 'supplierName', key: 'supplierName', responsive: ['lg'] as Array<'lg'> },
      {
        title: 'Số tiền',
        key: 'requestedAmount',
        align: 'right' as const,
        render: (_: unknown, record: PaymentRequest) => formatCurrency(record.requestedAmount),
      },
      {
        title: 'Hạn thanh toán',
        key: 'dueDate',
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: PaymentRequest) => formatDate(record.dueDate),
      },
      {
        title: 'Trạng thái',
        key: 'status',
        render: (_: unknown, record: PaymentRequest) => <StatusBadge status={record.currentStatus} />,
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: PaymentRequest) => (
          <Space wrap className="table-actions">
            <Button icon={<EyeOutlined />} onClick={() => navigate(`/payment-requests/${record.id}`)} />
            {canApprove(user?.role, record.currentStatus) && (
              <Button
                type="primary"
                icon={<CheckOutlined />}
                onClick={() => setModal({ open: true, action: 'approve', record })}
              >
                Duyệt
              </Button>
            )}
            {canRejectOrReturn(user?.role, record.currentStatus) && (
              <Button
                danger
                icon={<CloseOutlined />}
                onClick={() => setModal({ open: true, action: 'reject', record })}
              >
                Từ chối
              </Button>
            )}
            {canRejectOrReturn(user?.role, record.currentStatus) && (
              <Button icon={<EditOutlined />} onClick={() => setModal({ open: true, action: 'return', record })}>
                Trả sửa
              </Button>
            )}
          </Space>
        ),
      },
    ],
    [navigate, user?.role],
  );

  const actionTitle =
    modal.action === 'approve'
      ? 'Duyệt hồ sơ'
      : modal.action === 'reject'
        ? 'Từ chối hồ sơ'
        : 'Trả hồ sơ để chỉnh sửa';

  const commentRequired = modal.action === 'reject' || modal.action === 'return';

  return (
    <div className="page-stack">
      <div className="page-header">
        <div className="page-header__content">
          <Typography.Title level={3} style={{ margin: 0 }}>
            Duyệt hồ sơ thanh toán
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Danh sách duyệt đã được tinh gọn để thao tác nhanh hơn trên mobile, vẫn giữ nguyên luồng phê duyệt.
          </Typography.Text>
        </div>
      </div>

      <Card className="page-card">
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Select
            allowClear
            placeholder="Trạng thái"
            value={status}
            onChange={(value) => {
              setPageNumber(1);
              setStatus(value);
            }}
            options={paymentRequestStatusOptions.filter((x) =>
              ['PendingDepartmentApproval', 'PendingDirectorApproval', 'PendingAccounting'].includes(x.value),
            )}
          />
        </div>

        <Table<PaymentRequest>
          className="responsive-table"
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          scroll={{ x: 980 }}
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
        title={actionTitle}
        open={modal.open}
        onCancel={() => {
          setModal({ open: false, action: 'approve' });
          form.resetFields();
        }}
        onOk={() => form.submit()}
        confirmLoading={actionMutation.isPending}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={(values) => {
            if (!modal.record) {
              return;
            }

            actionMutation.mutate({
              action: modal.action,
              id: modal.record.id,
              comment: values.comment,
            });
          }}
        >
          <Typography.Paragraph>
            Hồ sơ: <strong>{modal.record?.requestCode}</strong>
          </Typography.Paragraph>

          <Form.Item
            label="Ý kiến"
            name="comment"
            rules={commentRequired ? [{ required: true, message: 'Vui lòng nhập lý do.' }] : []}
          >
            <Input.TextArea rows={4} placeholder="Nhập ý kiến xử lý" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
