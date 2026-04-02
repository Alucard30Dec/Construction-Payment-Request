import {
  AlertOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  DollarCircleOutlined,
  FileSearchOutlined,
  RiseOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Col, Progress, Row, Skeleton, Table, Tag, Typography } from 'antd';
import { useNavigate } from 'react-router-dom';
import { dashboardService } from '../../services/dashboardService';
import { formatCurrency, formatDate } from '../../utils/formatters';

export function DashboardPage() {
  const navigate = useNavigate();
  const summaryQuery = useQuery({
    queryKey: ['dashboard-summary'],
    queryFn: dashboardService.getSummary,
  });

  if (summaryQuery.isLoading) {
    return <Skeleton active />;
  }

  const summary = summaryQuery.data;
  if (!summary) {
    return <Typography.Text>Không có dữ liệu dashboard.</Typography.Text>;
  }

  return (
    <div className="page-stack">
      <Card className="page-card dashboard-hero">
        <div className="dashboard-hero__content">
          <Typography.Title level={3} style={{ marginBottom: 4 }}>
            Bảng điều khiển hồ sơ thanh toán
          </Typography.Title>
          <Typography.Text type="secondary">
            Theo dõi tình trạng duyệt, công nợ đến hạn và tiến độ thanh toán theo thời gian thực.
          </Typography.Text>
        </div>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} md={12} xl={6}>
          <Card className="page-card dashboard-kpi">
            <Typography.Text type="secondary">Tổng hồ sơ</Typography.Text>
            <Typography.Title level={3}>{summary.totalRequests}</Typography.Title>
            <Tag icon={<FileSearchOutlined />} color="blue">
              Đang vận hành
            </Tag>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={6}>
          <Card className="page-card dashboard-kpi">
            <Typography.Text type="secondary">Chờ duyệt</Typography.Text>
            <Typography.Title level={3}>{summary.pendingApprovalCount}</Typography.Title>
            <Tag icon={<ClockCircleOutlined />} color="orange">
              Cần xử lý
            </Tag>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={6}>
          <Card className="page-card dashboard-kpi">
            <Typography.Text type="secondary">Đã thanh toán</Typography.Text>
            <Typography.Title level={3}>{summary.paidCount}</Typography.Title>
            <Tag icon={<CheckCircleOutlined />} color="green">
              Hoàn tất
            </Tag>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={6}>
          <Card className="page-card dashboard-kpi">
            <Typography.Text type="secondary">Tổng tiền đề nghị</Typography.Text>
            <Typography.Title level={4}>{formatCurrency(summary.totalRequestedAmount)}</Typography.Title>
            <Typography.Text type="secondary">
              Đã thanh toán: {formatCurrency(summary.paidAmount)}
            </Typography.Text>
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={8}>
          <Card className="page-card" title="Tỷ lệ phê duyệt">
            <Progress
              type="dashboard"
              percent={Number(summary.approvalRatePercent)}
              strokeColor="#1677ff"
              format={(percent) => `${percent}%`}
            />
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
              Tỷ lệ hồ sơ đã qua bước duyệt nghiệp vụ và chuyển kế toán.
            </Typography.Paragraph>
          </Card>
        </Col>

        <Col xs={24} xl={8}>
          <Card className="page-card" title="Tỷ lệ thanh toán">
            <Progress
              type="dashboard"
              percent={Number(summary.paidRatePercent)}
              strokeColor="#16a34a"
              format={(percent) => `${percent}%`}
            />
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
              Tỷ lệ hồ sơ đã xác nhận thanh toán so với tổng hồ sơ.
            </Typography.Paragraph>
          </Card>
        </Col>

        <Col xs={24} xl={8}>
          <Card className="page-card" title="Cảnh báo tiến độ">
            <Typography.Paragraph style={{ marginBottom: 8 }}>
              <AlertOutlined style={{ color: '#ef4444', marginRight: 8 }} />
              Quá hạn: <strong>{summary.overdueCount}</strong>
            </Typography.Paragraph>
            <Typography.Paragraph style={{ marginBottom: 8 }}>
              <ClockCircleOutlined style={{ color: '#f59e0b', marginRight: 8 }} />
              Đến hạn 7 ngày: <strong>{summary.dueSoonCount}</strong>
            </Typography.Paragraph>
            <Typography.Paragraph style={{ marginBottom: 0 }}>
              <RiseOutlined style={{ color: '#0ea5e9', marginRight: 8 }} />
              TB giờ duyệt: <strong>{summary.averageApprovalHours}</strong>
            </Typography.Paragraph>
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <Card className="page-card" title="Tổng hồ sơ theo trạng thái">
            <Table
              className="responsive-table"
              rowKey="status"
              size="small"
              pagination={false}
              scroll={{ x: 420 }}
              dataSource={summary.statusSummaries}
              columns={[
                { title: 'Trạng thái', dataIndex: 'status', key: 'status' },
                { title: 'Số lượng', dataIndex: 'count', key: 'count', align: 'right' as const },
              ]}
            />
          </Card>
        </Col>

        <Col xs={24} xl={12}>
          <Card className="page-card" title="Tổng tiền theo tháng (12 tháng)">
            <Table
              className="responsive-table"
              rowKey={(record) => `${record.year}-${record.month}`}
              size="small"
              pagination={false}
              scroll={{ x: 420 }}
              dataSource={summary.monthlyAmountSummaries}
              columns={[
                {
                  title: 'Tháng',
                  key: 'month',
                  render: (_, record) => `${record.month}/${record.year}`,
                },
                {
                  title: 'Tổng tiền',
                  key: 'totalAmount',
                  align: 'right',
                  render: (_, record) => (
                    <Typography.Text strong>{formatCurrency(record.totalAmount)}</Typography.Text>
                  ),
                },
              ]}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <Card className="page-card" title="Top dự án theo giá trị thanh toán">
            <Table
              className="responsive-table"
              rowKey="id"
              size="small"
              pagination={false}
              scroll={{ x: 420 }}
              dataSource={summary.amountByProject}
              columns={[
                { title: 'Dự án', dataIndex: 'name', key: 'name' },
                {
                  title: 'Tổng tiền',
                  key: 'totalAmount',
                  align: 'right',
                  render: (_, record) => formatCurrency(record.totalAmount),
                },
              ]}
            />
          </Card>
        </Col>
        <Col xs={24} xl={12}>
          <Card className="page-card" title="Top nhà cung cấp theo giá trị thanh toán">
            <Table
              className="responsive-table"
              rowKey="id"
              size="small"
              pagination={false}
              scroll={{ x: 420 }}
              dataSource={summary.amountBySupplier}
              columns={[
                { title: 'Nhà cung cấp', dataIndex: 'name', key: 'name' },
                {
                  title: 'Tổng tiền',
                  key: 'totalAmount',
                  align: 'right',
                  render: (_, record) => formatCurrency(record.totalAmount),
                },
              ]}
            />
          </Card>
        </Col>
      </Row>

      <Card className="page-card" title="Top hồ sơ quá hạn cần ưu tiên xử lý">
        <Table
          className="responsive-table"
          rowKey="id"
          size="small"
          pagination={false}
          scroll={{ x: 980 }}
          dataSource={summary.topOverdueRequests}
          locale={{ emptyText: 'Không có hồ sơ quá hạn' }}
          columns={[
            { title: 'Mã hồ sơ', dataIndex: 'requestCode', key: 'requestCode', responsive: ['sm'] },
            { title: 'Tiêu đề', dataIndex: 'title', key: 'title' },
            { title: 'Dự án', dataIndex: 'projectName', key: 'projectName', responsive: ['lg'] },
            { title: 'Nhà cung cấp', dataIndex: 'supplierName', key: 'supplierName', responsive: ['lg'] },
            {
              title: 'Hạn thanh toán',
              key: 'dueDate',
              responsive: ['md'],
              render: (_, record) => formatDate(record.dueDate),
            },
            {
              title: 'Quá hạn (ngày)',
              dataIndex: 'overdueDays',
              key: 'overdueDays',
              align: 'right',
            },
            {
              title: 'Số tiền',
              key: 'requestedAmount',
              align: 'right',
              render: (_, record) => formatCurrency(record.requestedAmount),
            },
            {
              title: 'Chi tiết',
              key: 'actions',
              render: (_, record) => (
                <Button
                  size="small"
                  icon={<DollarCircleOutlined />}
                  onClick={() => navigate(`/payment-requests/${record.id}`)}
                >
                  Mở hồ sơ
                </Button>
              ),
            },
          ]}
        />
      </Card>
    </div>
  );
}
