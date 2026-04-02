import { EditOutlined, KeyOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Form, Input, Modal, Select, Space, Switch, Table, Typography, message } from 'antd';
import { useMemo, useState } from 'react';
import { userService, type CreateUserPayload, type UpdateUserPayload } from '../../services/userService';
import type { User, UserRole } from '../../types';
import { getErrorMessage } from '../../utils/apiError';
import { formatDateTime } from '../../utils/formatters';

interface UserFilterState {
  search: string;
  role?: UserRole;
  isActive?: boolean;
  pageNumber: number;
  pageSize: number;
}

interface UserFormValues {
  username?: string;
  fullName: string;
  email?: string;
  password?: string;
  role: UserRole;
  roleProfileId?: string;
  department?: string;
  isActive: boolean;
}

interface UserModalState {
  open: boolean;
  mode: 'create' | 'edit';
  record?: User;
}

interface ResetPasswordValues {
  newPassword: string;
}

export function UserManagementPage() {
  const queryClient = useQueryClient();
  const [userForm] = Form.useForm<UserFormValues>();
  const [resetPasswordForm] = Form.useForm<ResetPasswordValues>();

  const [filters, setFilters] = useState<UserFilterState>({
    search: '',
    role: undefined,
    isActive: undefined,
    pageNumber: 1,
    pageSize: 10,
  });

  const [userModal, setUserModal] = useState<UserModalState>({ open: false, mode: 'create' });
  const [resetPasswordUser, setResetPasswordUser] = useState<User | undefined>(undefined);

  const roleQuery = useQuery({
    queryKey: ['user-roles'],
    queryFn: userService.getRoles,
  });

  const roleProfileQuery = useQuery({
    queryKey: ['role-profiles'],
    queryFn: userService.getRoleProfiles,
  });

  const userQuery = useQuery({
    queryKey: ['users', filters],
    queryFn: () =>
      userService.getPaged({
        search: filters.search || undefined,
        role: filters.role,
        isActive: filters.isActive,
        pageNumber: filters.pageNumber,
        pageSize: filters.pageSize,
      }),
  });

  const createUserMutation = useMutation({
    mutationFn: (payload: CreateUserPayload) => userService.create(payload),
    onSuccess: () => {
      message.success('Tạo người dùng thành công.');
      setUserModal({ open: false, mode: 'create' });
      userForm.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const updateUserMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateUserPayload }) =>
      userService.update(id, payload),
    onSuccess: () => {
      message.success('Cập nhật người dùng thành công.');
      setUserModal({ open: false, mode: 'create' });
      userForm.resetFields();
      void queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const resetPasswordMutation = useMutation({
    mutationFn: ({ id, newPassword }: { id: string; newPassword: string }) =>
      userService.resetPassword(id, newPassword),
    onSuccess: () => {
      message.success('Đặt lại mật khẩu thành công.');
      setResetPasswordUser(undefined);
      resetPasswordForm.resetFields();
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const roleOptions = useMemo(
    () => (roleQuery.data ?? []).map((x) => ({ label: x.name, value: x.value })),
    [roleQuery.data],
  );

  const openCreateModal = () => {
    setUserModal({ open: true, mode: 'create' });
    userForm.setFieldsValue({
      isActive: true,
      role: 'Employee',
      username: '',
      fullName: '',
      email: '',
      password: '',
      roleProfileId: undefined,
      department: '',
    });
  };

  const openEditModal = (record: User) => {
    setUserModal({ open: true, mode: 'edit', record });
    userForm.setFieldsValue({
      fullName: record.fullName,
      email: record.email,
      role: record.role,
      roleProfileId: record.roleProfileId,
      department: record.department,
      isActive: record.isActive,
    });
  };

  const columns = useMemo(
    () => [
      { title: 'Tên đăng nhập', dataIndex: 'username', key: 'username' },
      { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName' },
      { title: 'Email', dataIndex: 'email', key: 'email', responsive: ['lg'] as Array<'lg'> },
      { title: 'Vai trò', dataIndex: 'role', key: 'role', responsive: ['md'] as Array<'md'> },
      { title: 'Role profile', dataIndex: 'roleProfileName', key: 'roleProfileName', responsive: ['xl'] as Array<'xl'> },
      { title: 'Phòng ban', dataIndex: 'department', key: 'department', responsive: ['lg'] as Array<'lg'> },
      {
        title: 'Trạng thái',
        key: 'isActive',
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: User) => (record.isActive ? 'Hoạt động' : 'Ngưng hoạt động'),
      },
      {
        title: 'Tạo lúc',
        key: 'createdAt',
        responsive: ['xl'] as Array<'xl'>,
        render: (_: unknown, record: User) => formatDateTime(record.createdAt),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: User) => (
          <Space wrap className="table-actions">
            <Button icon={<EditOutlined />} onClick={() => openEditModal(record)}>
              Sửa
            </Button>
            <Button icon={<KeyOutlined />} onClick={() => setResetPasswordUser(record)}>
              Đặt lại mật khẩu
            </Button>
          </Space>
        ),
      },
    ],
    [],
  );

  return (
    <div className="page-stack">
      <div className="page-header">
        <div className="page-header__content">
          <Typography.Title level={3} style={{ margin: 0 }}>
            Quản trị người dùng và phân quyền
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Tối ưu bảng quản trị và các modal nhập liệu để thao tác nhanh hơn trên màn hình hẹp.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
            Tạo người dùng
          </Button>
        </div>
      </div>

      <Card className="page-card">
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo username hoặc họ tên"
            onSearch={(value) =>
              setFilters((prev) => ({
                ...prev,
                search: value,
                pageNumber: 1,
              }))
            }
          />

          <Select
            allowClear
            placeholder="Vai trò"
            value={filters.role}
            onChange={(value) =>
              setFilters((prev) => ({
                ...prev,
                role: value,
                pageNumber: 1,
              }))
            }
            options={roleOptions}
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

        <Table<User>
          className="responsive-table"
          rowKey="id"
          loading={userQuery.isLoading}
          columns={columns}
          dataSource={userQuery.data?.items ?? []}
          scroll={{ x: 1180 }}
          pagination={{
            current: filters.pageNumber,
            pageSize: filters.pageSize,
            total: userQuery.data?.totalCount ?? 0,
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
        open={userModal.open}
        title={userModal.mode === 'create' ? 'Tạo người dùng' : 'Cập nhật người dùng'}
        onCancel={() => {
          setUserModal({ open: false, mode: 'create' });
          userForm.resetFields();
        }}
        onOk={() => userForm.submit()}
        confirmLoading={createUserMutation.isPending || updateUserMutation.isPending}
      >
        <Form<UserFormValues>
          form={userForm}
          layout="vertical"
          onFinish={(values) => {
            if (userModal.mode === 'create') {
              createUserMutation.mutate({
                username: values.username?.trim() ?? '',
                fullName: values.fullName,
                email: values.email,
                password: values.password ?? '',
                role: values.role,
                roleProfileId: values.roleProfileId,
                department: values.department,
                isActive: values.isActive,
              });
              return;
            }

            if (!userModal.record) {
              return;
            }

            updateUserMutation.mutate({
              id: userModal.record.id,
              payload: {
                fullName: values.fullName,
                email: values.email,
                role: values.role,
                roleProfileId: values.roleProfileId,
                department: values.department,
                isActive: values.isActive,
              },
            });
          }}
        >
          {userModal.mode === 'create' && (
            <>
              <Form.Item
                label="Tên đăng nhập"
                name="username"
                rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập.' }]}
              >
                <Input autoComplete="off" />
              </Form.Item>

              <Form.Item
                label="Mật khẩu"
                name="password"
                rules={[
                  { required: true, message: 'Vui lòng nhập mật khẩu.' },
                  { min: 8, message: 'Mật khẩu tối thiểu 8 ký tự.' },
                ]}
              >
                <Input.Password autoComplete="new-password" />
              </Form.Item>
            </>
          )}

          <Form.Item label="Họ và tên" name="fullName" rules={[{ required: true, message: 'Vui lòng nhập họ tên.' }]}>
            <Input />
          </Form.Item>

          <Form.Item label="Email" name="email" rules={[{ type: 'email', message: 'Email không hợp lệ.' }]}>
            <Input />
          </Form.Item>

          <Form.Item label="Vai trò" name="role" rules={[{ required: true, message: 'Vui lòng chọn vai trò.' }]}>
            <Select options={roleOptions} />
          </Form.Item>

          <Form.Item label="Role profile phân quyền" name="roleProfileId">
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Mặc định theo vai trò"
              options={(roleProfileQuery.data ?? [])
                .filter((x) => x.isActive)
                .map((x) => ({
                  label: `${x.name} (${x.code})`,
                  value: x.id,
                }))}
            />
          </Form.Item>

          <Form.Item label="Phòng ban" name="department">
            <Input />
          </Form.Item>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        open={Boolean(resetPasswordUser)}
        title={`Đặt lại mật khẩu: ${resetPasswordUser?.username ?? ''}`}
        onCancel={() => {
          setResetPasswordUser(undefined);
          resetPasswordForm.resetFields();
        }}
        onOk={() => resetPasswordForm.submit()}
        confirmLoading={resetPasswordMutation.isPending}
      >
        <Form<ResetPasswordValues>
          form={resetPasswordForm}
          layout="vertical"
          onFinish={(values) => {
            if (!resetPasswordUser) {
              return;
            }

            resetPasswordMutation.mutate({
              id: resetPasswordUser.id,
              newPassword: values.newPassword,
            });
          }}
        >
          <Form.Item
            label="Mật khẩu mới"
            name="newPassword"
            rules={[
              { required: true, message: 'Vui lòng nhập mật khẩu mới.' },
              { min: 8, message: 'Mật khẩu tối thiểu 8 ký tự.' },
            ]}
          >
            <Input.Password autoComplete="new-password" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
