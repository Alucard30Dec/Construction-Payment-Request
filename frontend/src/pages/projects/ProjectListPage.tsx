import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Input, Modal, Select, Space, Table, Typography, message } from 'antd';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Project } from '../../types';
import { projectService } from '../../services/projectService';
import { getErrorMessage } from '../../utils/apiError';

export function ProjectListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const query = useQuery({
    queryKey: ['projects', search, isActive, pageNumber, pageSize],
    queryFn: () =>
      projectService.getPaged({
        search: search || undefined,
        isActive,
        pageNumber,
        pageSize,
      }),
  });

  const removeMutation = useMutation({
    mutationFn: projectService.remove,
    onSuccess: () => {
      message.success('Đã xóa dự án.');
      void queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const columns = useMemo(
    () => [
      { title: 'Mã', dataIndex: 'code', key: 'code' },
      { title: 'Tên dự án', dataIndex: 'name', key: 'name' },
      { title: 'Địa điểm', dataIndex: 'location', key: 'location' },
      { title: 'Phòng ban', dataIndex: 'department', key: 'department' },
      { title: 'Quản lý dự án', dataIndex: 'projectManager', key: 'projectManager' },
      {
        title: 'Trạng thái',
        key: 'isActive',
        render: (_: unknown, record: Project) => (record.isActive ? 'Hoạt động' : 'Ngưng hoạt động'),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Project) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => navigate(`/projects/${record.id}/edit`)} />
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                Modal.confirm({
                  title: 'Xóa dự án',
                  content: `Bạn có chắc muốn xóa ${record.name}?`,
                  onOk: async () => removeMutation.mutateAsync(record.id),
                });
              }}
            />
          </Space>
        ),
      },
    ],
    [navigate, removeMutation],
  );

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Dự án
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/projects/new')}>
          Thêm mới
        </Button>
      </div>

      <Card>
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo mã hoặc tên dự án"
            onSearch={(value) => {
              setPageNumber(1);
              setSearch(value);
            }}
          />
          <Select
            allowClear
            placeholder="Trạng thái"
            value={isActive}
            onChange={(value) => {
              setPageNumber(1);
              setIsActive(value);
            }}
            options={[
              { label: 'Hoạt động', value: true },
              { label: 'Ngưng hoạt động', value: false },
            ]}
          />
        </div>

        <Table<Project>
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
