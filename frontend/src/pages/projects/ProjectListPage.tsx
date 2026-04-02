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
      { title: 'Mã', dataIndex: 'code', key: 'code', responsive: ['sm'] as Array<'sm'> },
      { title: 'Tên dự án', dataIndex: 'name', key: 'name' },
      { title: 'Địa điểm', dataIndex: 'location', key: 'location', responsive: ['lg'] as Array<'lg'> },
      { title: 'Phòng ban', dataIndex: 'department', key: 'department', responsive: ['md'] as Array<'md'> },
      {
        title: 'Quản lý dự án',
        dataIndex: 'projectManager',
        key: 'projectManager',
        responsive: ['xl'] as Array<'xl'>,
      },
      {
        title: 'Trạng thái',
        key: 'isActive',
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: Project) => (record.isActive ? 'Hoạt động' : 'Ngưng hoạt động'),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Project) => (
          <Space wrap className="table-actions">
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
    <div className="page-stack">
      <div className="page-header">
        <div className="page-header__content">
          <Typography.Title level={3} style={{ margin: 0 }}>
            Dự án
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Giữ lại đầy đủ dữ liệu, chỉ tối ưu bố cục và bảng hiển thị cho các màn hình nhỏ.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/projects/new')}>
            Thêm mới
          </Button>
        </div>
      </div>

      <Card className="page-card">
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
          className="responsive-table"
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          scroll={{ x: 820 }}
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
