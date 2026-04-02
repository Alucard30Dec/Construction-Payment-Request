import { EyeOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, DatePicker, Input, Modal, Select, Space, Table, Typography } from 'antd';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { auditLogService } from '../../services/auditLogService';
import type { AuditLog } from '../../types';
import { formatDateTime } from '../../utils/formatters';

interface FilterState {
  action?: string;
  entityName?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber: number;
  pageSize: number;
}

const actionOptions = [
  { label: 'Create', value: 'Create' },
  { label: 'Update', value: 'Update' },
  { label: 'Delete', value: 'Delete' },
  { label: 'Submit', value: 'Submit' },
  { label: 'Approve', value: 'Approve' },
  { label: 'Reject', value: 'Reject' },
  { label: 'ReturnForEdit', value: 'ReturnForEdit' },
  { label: 'ConfirmPayment', value: 'ConfirmPayment' },
  { label: 'UploadAttachment', value: 'UploadAttachment' },
  { label: 'DeleteAttachment', value: 'DeleteAttachment' },
  { label: 'ResetPassword', value: 'ResetPassword' },
];

const entityOptions = [
  { label: 'AppUser', value: 'AppUser' },
  { label: 'Supplier', value: 'Supplier' },
  { label: 'Project', value: 'Project' },
  { label: 'Contract', value: 'Contract' },
  { label: 'PaymentRequest', value: 'PaymentRequest' },
  { label: 'PaymentRequestAttachment', value: 'PaymentRequestAttachment' },
  { label: 'ApprovalMatrix', value: 'ApprovalMatrix' },
];

export function AuditLogPage() {
  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedLog, setSelectedLog] = useState<AuditLog | undefined>(undefined);
  const [filters, setFilters] = useState<FilterState>({
    action: undefined,
    entityName: undefined,
    fromDate: undefined,
    toDate: undefined,
    pageNumber: 1,
    pageSize: 20,
  });

  const query = useQuery({
    queryKey: ['audit-logs', searchKeyword, filters],
    queryFn: () =>
      auditLogService.getPaged({
        action: filters.action,
        entityName: filters.entityName,
        fromDate: filters.fromDate,
        toDate: filters.toDate,
        pageNumber: filters.pageNumber,
        pageSize: filters.pageSize,
      }),
  });

  const filteredRows = useMemo(() => {
    const items = query.data?.items ?? [];
    if (!searchKeyword.trim()) {
      return items;
    }

    const keyword = searchKeyword.trim().toLowerCase();
    return items.filter((x) =>
      [x.username, x.action, x.entityName, x.entityId]
        .filter(Boolean)
        .some((value) => value?.toLowerCase().includes(keyword)),
    );
  }, [query.data?.items, searchKeyword]);

  const columns = useMemo(
    () => [
      {
        title: 'Thời gian',
        key: 'createdAt',
        width: 170,
        render: (_: unknown, record: AuditLog) => formatDateTime(record.createdAt),
      },
      { title: 'Người thao tác', dataIndex: 'username', key: 'username', width: 140 },
      { title: 'Hành động', dataIndex: 'action', key: 'action', width: 160 },
      { title: 'Entity', dataIndex: 'entityName', key: 'entityName', width: 180, responsive: ['md'] as Array<'md'> },
      { title: 'EntityId', dataIndex: 'entityId', key: 'entityId', responsive: ['lg'] as Array<'lg'> },
      {
        title: 'Chi tiết',
        key: 'details',
        width: 90,
        align: 'center' as const,
        render: (_: unknown, record: AuditLog) => (
          <Button icon={<EyeOutlined />} onClick={() => setSelectedLog(record)} />
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
            Lịch sử Audit Log
          </Typography.Title>
          <Typography.Text className="page-subtitle">
            Dùng bộ lọc theo lưới responsive và modal xem chi tiết dễ đọc hơn trên màn hình nhỏ.
          </Typography.Text>
        </div>
      </div>

      <Card className="page-card">
        <div className="filter-grid" style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            placeholder="Lọc nhanh theo user/action/entity/id"
            onSearch={(value) => setSearchKeyword(value)}
          />

          <Select
            allowClear
            placeholder="Hành động"
            value={filters.action}
            onChange={(value) =>
              setFilters((prev) => ({
                ...prev,
                action: value,
                pageNumber: 1,
              }))
            }
            options={actionOptions}
          />

          <Select
            allowClear
            placeholder="Entity"
            value={filters.entityName}
            onChange={(value) =>
              setFilters((prev) => ({
                ...prev,
                entityName: value,
                pageNumber: 1,
              }))
            }
            options={entityOptions}
          />

          <Space className="inline-date-range" size={12} style={{ width: '100%' }}>
            <DatePicker
              placeholder="Từ ngày"
              format="DD/MM/YYYY"
              style={{ width: '100%' }}
              onChange={(value) =>
                setFilters((prev) => ({
                  ...prev,
                  fromDate: value ? dayjs(value).startOf('day').toISOString() : undefined,
                  pageNumber: 1,
                }))
              }
            />
            <DatePicker
              placeholder="Đến ngày"
              format="DD/MM/YYYY"
              style={{ width: '100%' }}
              onChange={(value) =>
                setFilters((prev) => ({
                  ...prev,
                  toDate: value ? dayjs(value).endOf('day').toISOString() : undefined,
                  pageNumber: 1,
                }))
              }
            />
          </Space>
        </div>

        <Table<AuditLog>
          className="responsive-table"
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={filteredRows}
          scroll={{ x: 900 }}
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
        open={Boolean(selectedLog)}
        title={`Chi tiết log: ${selectedLog?.id ?? ''}`}
        width={900}
        onCancel={() => setSelectedLog(undefined)}
        footer={null}
      >
        <Space direction="vertical" style={{ width: '100%' }} size={16}>
          <div>
            <Typography.Text strong>OldValue</Typography.Text>
            <pre className="modal-code-block">{selectedLog?.oldValue || 'null'}</pre>
          </div>

          <div>
            <Typography.Text strong>NewValue</Typography.Text>
            <pre className="modal-code-block">{selectedLog?.newValue || 'null'}</pre>
          </div>
        </Space>
      </Modal>
    </div>
  );
}
