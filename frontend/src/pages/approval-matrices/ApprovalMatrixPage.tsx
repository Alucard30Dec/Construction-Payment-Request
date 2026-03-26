import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Space,
  Switch,
  Table,
  Typography,
  message,
} from 'antd';
import { useMemo, useState } from 'react';
import {
  approvalMatrixService,
  type ApprovalMatrixPayload,
} from '../../services/approvalMatrixService';
import type { ApprovalMatrix } from '../../types';
import { useProjectLookup } from '../../hooks/useLookups';
import { getErrorMessage } from '../../utils/apiError';
import { formatCurrency, formatDateTime } from '../../utils/formatters';

interface FilterState {
  department?: string;
  projectId?: string;
  isActive?: boolean;
  pageNumber: number;
  pageSize: number;
}

interface MatrixModalState {
  open: boolean;
  mode: 'create' | 'edit';
  record?: ApprovalMatrix;
}

type MatrixFormValues = ApprovalMatrixPayload;

export function ApprovalMatrixPage() {
  const queryClient = useQueryClient();
  const [form] = Form.useForm<MatrixFormValues>();
  const projectLookup = useProjectLookup();

  const [filters, setFilters] = useState<FilterState>({
    department: undefined,
    projectId: undefined,
    isActive: undefined,
    pageNumber: 1,
    pageSize: 10,
  });
  const [modalState, setModalState] = useState<MatrixModalState>({
    open: false,
    mode: 'create',
  });

  const query = useQuery({
    queryKey: ['approval-matrices', filters],
    queryFn: () =>
      approvalMatrixService.getPaged({
        department: filters.department,
        projectId: filters.projectId,
        isActive: filters.isActive,
        pageNumber: filters.pageNumber,
        pageSize: filters.pageSize,
      }),
  });

  const createMutation = useMutation({
    mutationFn: (payload: ApprovalMatrixPayload) => approvalMatrixService.create(payload),
    onSuccess: () => {
      message.success('Tạo ma trận duyệt thành công.');
      setModalState({ open: false, mode: 'create' });
      form.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['approval-matrices'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ApprovalMatrixPayload }) =>
      approvalMatrixService.update(id, payload),
    onSuccess: () => {
      message.success('Cập nhật ma trận duyệt thành công.');
      setModalState({ open: false, mode: 'create' });
      form.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['approval-matrices'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const deleteMutation = useMutation({
    mutationFn: approvalMatrixService.remove,
    onSuccess: () => {
      message.success('Đã xóa ma trận duyệt.');
      void queryClient.invalidateQueries({ queryKey: ['approval-matrices'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const openCreateModal = () => {
    setModalState({ open: true, mode: 'create' });
    form.setFieldsValue({
      minAmount: 0,
      maxAmount: 1000000,
      requireDirectorApproval: false,
      department: '',
      projectId: undefined,
      isActive: true,
    });
  };

  const openEditModal = (record: ApprovalMatrix) => {
    setModalState({ open: true, mode: 'edit', record });
    form.setFieldsValue({
      minAmount: record.minAmount,
      maxAmount: record.maxAmount,
      requireDirectorApproval: record.requireDirectorApproval,
      department: record.department,
      projectId: record.projectId,
      isActive: record.isActive,
    });
  };

  const columns = useMemo(
    () => [
      {
        title: 'Khoảng tiền',
        key: 'amountRange',
        render: (_: unknown, record: ApprovalMatrix) =>
          `${formatCurrency(record.minAmount)} - ${formatCurrency(record.maxAmount)}`,
      },
      { title: 'Phòng ban', dataIndex: 'department', key: 'department' },
      { title: 'Dự án', dataIndex: 'projectName', key: 'projectName' },
      {
        title: 'Yêu cầu duyệt GĐ',
        key: 'requireDirectorApproval',
        render: (_: unknown, record: ApprovalMatrix) =>
          record.requireDirectorApproval ? 'Có' : 'Không',
      },
      {
        title: 'Trạng thái',
        key: 'isActive',
        render: (_: unknown, record: ApprovalMatrix) =>
          record.isActive ? 'Hoạt động' : 'Ngưng hoạt động',
      },
      {
        title: 'Cập nhật',
        key: 'updatedAt',
        render: (_: unknown, record: ApprovalMatrix) => formatDateTime(record.updatedAt),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: ApprovalMatrix) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => openEditModal(record)}>
              Sửa
            </Button>
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() =>
                Modal.confirm({
                  title: 'Xóa ma trận duyệt',
                  content: 'Bạn có chắc muốn xóa cấu hình này?',
                  onOk: async () => deleteMutation.mutateAsync(record.id),
                })
              }
            >
              Xóa
            </Button>
          </Space>
        ),
      },
    ],
    [deleteMutation],
  );

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Cấu hình ma trận duyệt
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
          Thêm cấu hình
        </Button>
      </div>

      <Card>
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input
            allowClear
            placeholder="Phòng ban"
            value={filters.department}
            onChange={(event) =>
              setFilters((prev) => ({
                ...prev,
                department: event.target.value || undefined,
                pageNumber: 1,
              }))
            }
          />

          <Select
            allowClear
            placeholder="Dự án"
            value={filters.projectId}
            onChange={(value) =>
              setFilters((prev) => ({
                ...prev,
                projectId: value,
                pageNumber: 1,
              }))
            }
            options={(projectLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
          />

          <Select
            allowClear
            placeholder="Trạng thái"
            value={filters.isActive}
            onChange={(value) =>
              setFilters((prev) => ({
                ...prev,
                isActive: value,
                pageNumber: 1,
              }))
            }
            options={[
              { label: 'Hoạt động', value: true },
              { label: 'Ngưng hoạt động', value: false },
            ]}
          />
        </div>

        <Table<ApprovalMatrix>
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          pagination={{
            current: filters.pageNumber,
            pageSize: filters.pageSize,
            total: query.data?.totalCount ?? 0,
            showSizeChanger: true,
            onChange: (page, size) =>
              setFilters((prev) => ({
                ...prev,
                pageNumber: page,
                pageSize: size,
              })),
          }}
        />
      </Card>

      <Modal
        open={modalState.open}
        title={modalState.mode === 'create' ? 'Tạo ma trận duyệt' : 'Cập nhật ma trận duyệt'}
        onCancel={() => {
          setModalState({ open: false, mode: 'create' });
          form.resetFields();
        }}
        onOk={() => form.submit()}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
      >
        <Form<MatrixFormValues>
          form={form}
          layout="vertical"
          onFinish={(values) => {
            const payload: ApprovalMatrixPayload = {
              minAmount: values.minAmount,
              maxAmount: values.maxAmount,
              requireDirectorApproval: values.requireDirectorApproval,
              department: values.department,
              projectId: values.projectId,
              isActive: values.isActive,
            };

            if (modalState.mode === 'create') {
              createMutation.mutate(payload);
              return;
            }

            if (!modalState.record) {
              return;
            }

            updateMutation.mutate({
              id: modalState.record.id,
              payload,
            });
          }}
        >
          <Form.Item
            label="Giá trị tối thiểu"
            name="minAmount"
            rules={[{ required: true, message: 'Vui lòng nhập giá trị tối thiểu.' }]}
          >
            <InputNumber style={{ width: '100%' }} min={0} step={1000000} />
          </Form.Item>

          <Form.Item
            label="Giá trị tối đa"
            name="maxAmount"
            rules={[
              { required: true, message: 'Vui lòng nhập giá trị tối đa.' },
              ({ getFieldValue }) => ({
                validator(_, value: number | undefined) {
                  const minAmount = getFieldValue('minAmount') as number | undefined;
                  if (
                    typeof value !== 'number' ||
                    typeof minAmount !== 'number' ||
                    value > minAmount
                  ) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('Giá trị tối đa phải lớn hơn giá trị tối thiểu.'));
                },
              }),
            ]}
          >
            <InputNumber style={{ width: '100%' }} min={0} step={1000000} />
          </Form.Item>

          <Form.Item label="Phòng ban" name="department">
            <Input />
          </Form.Item>

          <Form.Item label="Dự án (tùy chọn)" name="projectId">
            <Select
              allowClear
              options={(projectLookup.data ?? []).map((x) => ({ label: x.name, value: x.id }))}
            />
          </Form.Item>

          <Form.Item
            label="Yêu cầu giám đốc duyệt"
            name="requireDirectorApproval"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
