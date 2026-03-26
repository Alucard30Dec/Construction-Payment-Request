import { DeleteOutlined, EditOutlined, PlusOutlined, SaveOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  Checkbox,
  Col,
  Divider,
  Empty,
  Form,
  Input,
  Modal,
  Row,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Typography,
  message,
} from 'antd';
import { useEffect, useMemo, useState } from 'react';
import {
  rolePermissionService,
  type CreateRoleProfilePayload,
  type UpdateRoleProfilePayload,
} from '../../services/rolePermissionService';
import type { PermissionCatalogItem, RoleProfile } from '../../types';
import { getErrorMessage } from '../../utils/apiError';

interface RoleProfileModalState {
  open: boolean;
  mode: 'create' | 'edit';
  profile?: RoleProfile;
}

interface RoleProfileFormValues {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  cloneFromRoleProfileId?: string;
}

export function RolePermissionMatrixPage() {
  const queryClient = useQueryClient();
  const [form] = Form.useForm<RoleProfileFormValues>();
  const [modalState, setModalState] = useState<RoleProfileModalState>({ open: false, mode: 'create' });
  const [selectedProfileId, setSelectedProfileId] = useState<string | undefined>(undefined);
  const [grantedPermissionCodes, setGrantedPermissionCodes] = useState<string[]>([]);

  const profilesQuery = useQuery({
    queryKey: ['role-profiles'],
    queryFn: rolePermissionService.getProfiles,
  });

  const catalogQuery = useQuery({
    queryKey: ['permission-catalog'],
    queryFn: rolePermissionService.getCatalog,
  });

  useEffect(() => {
    if (!profilesQuery.data || profilesQuery.data.length === 0) {
      setSelectedProfileId(undefined);
      setGrantedPermissionCodes([]);
      return;
    }

    if (!selectedProfileId) {
      setSelectedProfileId(profilesQuery.data[0].id);
      return;
    }

    const selected = profilesQuery.data.find((x) => x.id === selectedProfileId);
    if (!selected) {
      setSelectedProfileId(profilesQuery.data[0].id);
    }
  }, [profilesQuery.data, selectedProfileId]);

  const selectedProfile = useMemo(
    () => profilesQuery.data?.find((x) => x.id === selectedProfileId),
    [profilesQuery.data, selectedProfileId],
  );

  useEffect(() => {
    if (!selectedProfile) {
      setGrantedPermissionCodes([]);
      return;
    }

    setGrantedPermissionCodes(selectedProfile.grantedPermissionCodes);
  }, [selectedProfile]);

  const groupedPermissions = useMemo(() => {
    const items = catalogQuery.data ?? [];
    return items.reduce<Record<string, PermissionCatalogItem[]>>((acc, permission) => {
      if (!acc[permission.group]) {
        acc[permission.group] = [];
      }
      acc[permission.group].push(permission);
      return acc;
    }, {});
  }, [catalogQuery.data]);

  const refreshRoleData = async () => {
    await queryClient.invalidateQueries({ queryKey: ['role-profiles'] });
    await queryClient.invalidateQueries({ queryKey: ['permission-catalog'] });
    await queryClient.invalidateQueries({ queryKey: ['users'] });
  };

  const createMutation = useMutation({
    mutationFn: (payload: CreateRoleProfilePayload) => rolePermissionService.create(payload),
    onSuccess: async (data) => {
      message.success('Tạo role profile thành công.');
      setModalState({ open: false, mode: 'create' });
      form.resetFields();
      await refreshRoleData();
      setSelectedProfileId(data.id);
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateRoleProfilePayload }) =>
      rolePermissionService.update(id, payload),
    onSuccess: async () => {
      message.success('Cập nhật role profile thành công.');
      setModalState({ open: false, mode: 'create' });
      form.resetFields();
      await refreshRoleData();
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const deleteMutation = useMutation({
    mutationFn: rolePermissionService.remove,
    onSuccess: async () => {
      message.success('Xóa role profile thành công.');
      await refreshRoleData();
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const savePermissionsMutation = useMutation({
    mutationFn: ({ id, permissionCodes }: { id: string; permissionCodes: string[] }) =>
      rolePermissionService.savePermissions(id, { grantedPermissionCodes: permissionCodes }),
    onSuccess: async () => {
      message.success('Lưu ma trận phân quyền thành công.');
      await refreshRoleData();
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const openCreateModal = () => {
    setModalState({ open: true, mode: 'create' });
    form.setFieldsValue({
      code: '',
      name: '',
      description: '',
      isActive: true,
      cloneFromRoleProfileId: selectedProfileId,
    });
  };

  const openEditModal = (profile: RoleProfile) => {
    setModalState({ open: true, mode: 'edit', profile });
    form.setFieldsValue({
      code: profile.code,
      name: profile.name,
      description: profile.description,
      isActive: profile.isActive,
      cloneFromRoleProfileId: undefined,
    });
  };

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Quản trị Role và Ma trận phân quyền
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
          Tạo role profile
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={10}>
          <Card title="Danh sách role profile" loading={profilesQuery.isLoading}>
            <Table<RoleProfile>
              rowKey="id"
              size="small"
              dataSource={profilesQuery.data ?? []}
              pagination={false}
              rowClassName={(record) => (record.id === selectedProfileId ? 'selected-table-row' : '')}
              onRow={(record) => ({
                onClick: () => setSelectedProfileId(record.id),
                style: { cursor: 'pointer' },
              })}
              columns={[
                {
                  title: 'Role profile',
                  key: 'name',
                  render: (_, record) => (
                    <Space direction="vertical" size={0}>
                      <Typography.Text strong>{record.name}</Typography.Text>
                      <Typography.Text type="secondary">{record.code}</Typography.Text>
                    </Space>
                  ),
                },
                {
                  title: 'Trạng thái',
                  key: 'isActive',
                  render: (_, record) => (
                    <Tag color={record.isActive ? 'success' : 'default'}>
                      {record.isActive ? 'Active' : 'Inactive'}
                    </Tag>
                  ),
                },
                {
                  title: 'Users',
                  dataIndex: 'userCount',
                  key: 'userCount',
                  width: 80,
                },
                {
                  title: 'Thao tác',
                  key: 'actions',
                  width: 140,
                  render: (_, record) => (
                    <Space>
                      <Button
                        icon={<EditOutlined />}
                        onClick={(event) => {
                          event.stopPropagation();
                          openEditModal(record);
                        }}
                      />
                      {!record.isSystem && (
                        <Button
                          danger
                          icon={<DeleteOutlined />}
                          loading={deleteMutation.isPending}
                          onClick={(event) => {
                            event.stopPropagation();
                            Modal.confirm({
                              title: 'Xóa role profile',
                              content: `Bạn có chắc muốn xóa "${record.name}"?`,
                              onOk: () => deleteMutation.mutate(record.id),
                            });
                          }}
                        />
                      )}
                    </Space>
                  ),
                },
              ]}
            />
          </Card>
        </Col>

        <Col xs={24} xl={14}>
          <Card
            title={
              selectedProfile ? (
                <Space align="center">
                  <Typography.Text strong>{selectedProfile.name}</Typography.Text>
                  <Tag>{selectedProfile.code}</Tag>
                  {selectedProfile.isSystem && <Tag color="blue">System</Tag>}
                </Space>
              ) : (
                'Ma trận quyền'
              )
            }
            extra={
              selectedProfile ? (
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  loading={savePermissionsMutation.isPending}
                  disabled={!selectedProfile.isActive}
                  onClick={() =>
                    savePermissionsMutation.mutate({
                      id: selectedProfile.id,
                      permissionCodes: grantedPermissionCodes,
                    })
                  }
                >
                  Lưu phân quyền
                </Button>
              ) : null
            }
            loading={catalogQuery.isLoading}
          >
            {!selectedProfile && <Empty description="Chọn role profile để thiết lập phân quyền." />}

            {selectedProfile && (
              <Space direction="vertical" size={16} style={{ width: '100%' }}>
                {!selectedProfile.isActive && (
                  <Typography.Text type="warning">
                    Role profile đang bị vô hiệu hóa. Vui lòng bật Active trước khi thay đổi quyền.
                  </Typography.Text>
                )}

                {Object.entries(groupedPermissions).map(([groupName, permissions]) => (
                  <div key={groupName}>
                    <Typography.Title level={5} style={{ marginBottom: 8 }}>
                      {groupName}
                    </Typography.Title>
                    <Row gutter={[12, 12]}>
                      {permissions.map((permission) => (
                        <Col xs={24} md={12} key={permission.code}>
                          <Card size="small" className="permission-card">
                            <Space direction="vertical" size={4} style={{ width: '100%' }}>
                              <Checkbox
                                checked={grantedPermissionCodes.includes(permission.code)}
                                disabled={!selectedProfile.isActive}
                                onChange={(event) => {
                                  setGrantedPermissionCodes((prev) => {
                                    if (event.target.checked) {
                                      return Array.from(new Set([...prev, permission.code]));
                                    }

                                    return prev.filter((x) => x !== permission.code);
                                  });
                                }}
                              >
                                <Typography.Text strong>{permission.name}</Typography.Text>
                              </Checkbox>
                              <Typography.Text type="secondary">{permission.description}</Typography.Text>
                              <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                                {permission.code}
                              </Typography.Text>
                            </Space>
                          </Card>
                        </Col>
                      ))}
                    </Row>
                    <Divider />
                  </div>
                ))}
              </Space>
            )}
          </Card>
        </Col>
      </Row>

      <Modal
        open={modalState.open}
        title={modalState.mode === 'create' ? 'Tạo role profile' : 'Cập nhật role profile'}
        onCancel={() => {
          setModalState({ open: false, mode: 'create' });
          form.resetFields();
        }}
        onOk={() => form.submit()}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
      >
        <Form<RoleProfileFormValues>
          form={form}
          layout="vertical"
          onFinish={(values) => {
            if (modalState.mode === 'create') {
              createMutation.mutate({
                code: values.code.trim(),
                name: values.name.trim(),
                description: values.description?.trim(),
                isActive: values.isActive,
                cloneFromRoleProfileId: values.cloneFromRoleProfileId,
                grantedPermissionCodes: [],
              });
              return;
            }

            if (!modalState.profile) {
              return;
            }

            updateMutation.mutate({
              id: modalState.profile.id,
              payload: {
                code: values.code.trim(),
                name: values.name.trim(),
                description: values.description?.trim(),
                isActive: values.isActive,
              },
            });
          }}
        >
          <Form.Item
            label="Mã role profile"
            name="code"
            rules={[{ required: true, message: 'Vui lòng nhập mã role profile.' }]}
          >
            <Input disabled={Boolean(modalState.profile?.isSystem)} />
          </Form.Item>

          <Form.Item
            label="Tên hiển thị"
            name="name"
            rules={[{ required: true, message: 'Vui lòng nhập tên hiển thị.' }]}
          >
            <Input />
          </Form.Item>

          {modalState.mode === 'create' && (
            <Form.Item label="Clone phân quyền từ role profile" name="cloneFromRoleProfileId">
              <Select
                allowClear
                placeholder="Không clone (tạo rỗng)"
                options={(profilesQuery.data ?? []).map((x) => ({
                  label: `${x.name} (${x.code})`,
                  value: x.id,
                }))}
              />
            </Form.Item>
          )}

          <Form.Item label="Mô tả" name="description">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
