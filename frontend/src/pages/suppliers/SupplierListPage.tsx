import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Input, Modal, Select, Space, Table, Typography, message } from 'antd';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Supplier } from '../../types';
import { supplierService } from '../../services/supplierService';
import { getErrorMessage } from '../../utils/apiError';

export function SupplierListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const query = useQuery({
    queryKey: ['suppliers', search, isActive, pageNumber, pageSize],
    queryFn: () =>
      supplierService.getPaged({
        search: search || undefined,
        isActive,
        pageNumber,
        pageSize,
      }),
  });

  const removeMutation = useMutation({
    mutationFn: supplierService.remove,
    onSuccess: () => {
      message.success('Đã xóa nhà cung cấp.');
      void queryClient.invalidateQueries({ queryKey: ['suppliers'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const columns = useMemo(
    () => [
      { title: 'Mã', dataIndex: 'code', key: 'code', responsive: ['sm'] as Array<'sm'> },
      { title: 'Tên nhà cung cấp', dataIndex: 'name', key: 'name' },
      { title: 'Mã số thuế', dataIndex: 'taxCode', key: 'taxCode', responsive: ['lg'] as Array<'lg'> },
      { title: 'Liên hệ', dataIndex: 'contactPerson', key: 'contactPerson', responsive: ['xl'] as Array<'xl'> },
      { title: 'Điện thoại', dataIndex: 'phone', key: 'phone', responsive: ['lg'] as Array<'lg'> },
      {
        title: 'Trạng thái',
        key: 'isActive',
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: Supplier) => (record.isActive ? 'Hoạt động' : 'Ngưng hoạt động'),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Supplier) => (
          <Space wrap className="table-actions">
            <Button icon={<EditOutlined />} onClick={() => navigate(`/suppliers/${record.id}/edit`)} />
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                Modal.confirm({
                  title: 'Xóa nhà cung cấp',
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
            Nhà cung cấp
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Quản lý danh mục nhà cung cấp với bộ lọc gọn, dễ thao tác trên cả desktop và mobile.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/suppliers/new')}>
            Thêm mới
          </Button>
        </div>
      </div>

      <Card className="page-card">
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo mã, tên, mã số thuế"
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

        <Table<Supplier>
          className="responsive-table"
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          scroll={{ x: 760 }}
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
