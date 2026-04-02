import { DeleteOutlined, EditOutlined, EyeOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Input, Modal, Select, Space, Table, Typography, message } from 'antd';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Contract } from '../../types';
import { contractService } from '../../services/contractService';
import { useProjectLookup, useSupplierLookup } from '../../hooks/useLookups';
import { getErrorMessage } from '../../utils/apiError';
import { formatCurrency, formatDate } from '../../utils/formatters';

export function ContractListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const supplierLookup = useSupplierLookup();
  const projectLookup = useProjectLookup();

  const [search, setSearch] = useState('');
  const [supplierId, setSupplierId] = useState<string | undefined>(undefined);
  const [projectId, setProjectId] = useState<string | undefined>(undefined);
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const query = useQuery({
    queryKey: ['contracts', search, supplierId, projectId, isActive, pageNumber, pageSize],
    queryFn: () =>
      contractService.getPaged({
        search: search || undefined,
        supplierId,
        projectId,
        isActive,
        pageNumber,
        pageSize,
      }),
  });

  const removeMutation = useMutation({
    mutationFn: contractService.remove,
    onSuccess: () => {
      message.success('Đã xóa hợp đồng.');
      void queryClient.invalidateQueries({ queryKey: ['contracts'] });
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  const columns = useMemo(
    () => [
      {
        title: 'Số hợp đồng',
        dataIndex: 'contractNumber',
        key: 'contractNumber',
        render: (value: string, record: Contract) => (
          <Button type="link" style={{ padding: 0 }} onClick={() => navigate(`/contracts/${record.id}`)}>
            {value}
          </Button>
        ),
      },
      { title: 'Tên hợp đồng', dataIndex: 'name', key: 'name' },
      { title: 'Nhà cung cấp', dataIndex: 'supplierName', key: 'supplierName', responsive: ['lg'] as Array<'lg'> },
      { title: 'Dự án', dataIndex: 'projectName', key: 'projectName', responsive: ['lg'] as Array<'lg'> },
      {
        title: 'Ngày ký',
        key: 'signedDate',
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: Contract) => formatDate(record.signedDate),
      },
      {
        title: 'Giá trị',
        key: 'contractValue',
        align: 'right' as const,
        render: (_: unknown, record: Contract) => formatCurrency(record.contractValue),
      },
      {
        title: 'Tệp',
        key: 'attachmentCount',
        align: 'center' as const,
        responsive: ['md'] as Array<'md'>,
        render: (_: unknown, record: Contract) => record.attachmentCount ?? 0,
      },
      {
        title: 'Thao tác',
        key: 'actions',
        render: (_: unknown, record: Contract) => (
          <Space wrap className="table-actions">
            <Button icon={<EyeOutlined />} onClick={() => navigate(`/contracts/${record.id}`)} />
            <Button icon={<EditOutlined />} onClick={() => navigate(`/contracts/${record.id}/edit`)} />
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                Modal.confirm({
                  title: 'Xóa hợp đồng',
                  content: `Bạn có chắc muốn xóa ${record.contractNumber}?`,
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
            Hợp đồng
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Bảng hợp đồng ưu tiên thông tin chính trước, phần cột phụ sẽ tự ẩn trên màn hình nhỏ.
          </Typography.Text>
        </div>
        <div className="page-header__actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/contracts/new')}>
            Thêm mới
          </Button>
        </div>
      </div>

      <Card className="page-card">
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Tìm theo số hoặc tên hợp đồng"
            onSearch={(value) => {
              setPageNumber(1);
              setSearch(value);
            }}
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

        <Table<Contract>
          className="responsive-table"
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          scroll={{ x: 960 }}
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
