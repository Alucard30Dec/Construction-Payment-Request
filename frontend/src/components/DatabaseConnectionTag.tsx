import { DatabaseOutlined, ExclamationCircleOutlined, SyncOutlined } from '@ant-design/icons';
import { Tag, Tooltip } from 'antd';
import type { DatabaseHealth } from '../types';
import { useDatabaseHealth } from '../hooks/useDatabaseHealth';

function resolveProviderLabel(health: DatabaseHealth): string {
  const configured = health.configuredProvider?.toLowerCase() ?? '';
  const runtime = health.provider?.toLowerCase() ?? '';

  if (configured.includes('mysql') || runtime.includes('mysql')) {
    return 'TiDB (MySQL)';
  }

  if (configured.includes('sqlite') || runtime.includes('sqlite')) {
    return 'SQLite';
  }

  if (configured.includes('sqlserver') || runtime.includes('sqlserver')) {
    return 'SQL Server';
  }

  return health.configuredProvider || health.provider || 'Unknown';
}

export function DatabaseConnectionTag() {
  const dbHealthQuery = useDatabaseHealth();

  if (dbHealthQuery.isLoading) {
    return (
      <Tag icon={<SyncOutlined spin />} color="blue">
        Đang kiểm tra kết nối DB...
      </Tag>
    );
  }

  if (dbHealthQuery.isError || !dbHealthQuery.data) {
    return (
      <Tag icon={<ExclamationCircleOutlined />} color="red">
        API/DB chưa sẵn sàng
      </Tag>
    );
  }

  const health = dbHealthQuery.data;
  const providerLabel = resolveProviderLabel(health);

  if (health.status === 'ok') {
    return (
      <Tooltip
        title={`Configured: ${health.configuredProvider} | Runtime: ${health.provider}`}
        placement="bottom"
      >
        <Tag icon={<DatabaseOutlined />} color="green">
          Đang kết nối: {providerLabel}
        </Tag>
      </Tooltip>
    );
  }

  const statusText = health.status === 'schema_error' ? 'Lỗi schema DB' : 'DB chưa truy cập được';

  return (
    <Tooltip title={health.message ?? `Configured: ${health.configuredProvider} | Runtime: ${health.provider}`}>
      <Tag icon={<ExclamationCircleOutlined />} color="orange">
        {statusText} ({providerLabel})
      </Tag>
    </Tooltip>
  );
}
