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

  if (dbHealthQuery.isLoading || (!dbHealthQuery.data && dbHealthQuery.isFetching)) {
    return (
      <Tag icon={<SyncOutlined spin />} color="blue">
        Dang cho API/DB san sang...
      </Tag>
    );
  }

  if (dbHealthQuery.isError || !dbHealthQuery.data) {
    return (
      <Tag icon={dbHealthQuery.isFetching ? <SyncOutlined spin /> : <ExclamationCircleOutlined />} color="red">
        API/DB chua san sang
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
          Dang ket noi: {providerLabel}
        </Tag>
      </Tooltip>
    );
  }

  const statusText = health.status === 'schema_error' ? 'Loi schema DB' : 'DB chua truy cap duoc';

  return (
    <Tooltip title={health.message ?? `Configured: ${health.configuredProvider} | Runtime: ${health.provider}`}>
      <Tag icon={dbHealthQuery.isFetching ? <SyncOutlined spin /> : <ExclamationCircleOutlined />} color="orange">
        {statusText} ({providerLabel})
      </Tag>
    </Tooltip>
  );
}
