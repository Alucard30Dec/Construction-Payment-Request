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
      { title: 'Mã', dataIndex: 'code', key: 'code' },
      { title: 'Tên nhà cung cấp', dataIndex: 'name', key: 'name' },
      { title: 'Mã số thuế', dataIndex: 'taxCode', key: 'taxCode' },
      { title: 'Liên hệ', dataIndex: 'contactPerson', key: 'contactPerson' },
      { title: 'Điện thoại', dataIndex: 'phone', key: 'phone' },
      {
        title: 'Trạng thái',
        key: 'isActive',
        render: (_: unknown, record: Supplier) => (record.isActive ? 'Hoạt động' : 'Ngưng hoạt động'),
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Supplier) => (
          <Space>
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
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Nhà cung cấp
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/suppliers/new')}>
          Thêm mới
        </Button>
      </div>

      <Card>
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
