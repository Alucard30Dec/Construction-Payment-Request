import {
  AuditOutlined,
  BuildOutlined,
  DashboardOutlined,
  FileDoneOutlined,
  FileSearchOutlined,
  LogoutOutlined,
  ProjectOutlined,
  TeamOutlined,
  UserOutlined,
  WalletOutlined,
  ApartmentOutlined,
  SlidersOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import { Avatar, Button, Layout, Menu, Space, Typography } from 'antd';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import type { ItemType } from 'antd/es/menu/interface';
import { useMemo } from 'react';
import { useAuth } from '../hooks/useAuth';
import { PERMISSIONS } from '../constants/permissions';
import skyline from '../assets/skyline.svg';
import { DatabaseConnectionTag } from '../components/DatabaseConnectionTag';

const { Header, Sider, Content } = Layout;

export function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout, hasPermission } = useAuth();

  const menuItems = useMemo<ItemType[]>(() => {
    const items: ItemType[] = [];

    if (hasPermission(PERMISSIONS.dashboardView)) {
      items.push({
        key: '/dashboard',
        icon: <DashboardOutlined />,
        label: 'Dashboard',
      });
    }

    if (hasPermission(PERMISSIONS.suppliersView)) {
      items.push({
        key: '/suppliers',
        icon: <TeamOutlined />,
        label: 'Nhà cung cấp',
      });
    }

    if (hasPermission(PERMISSIONS.projectsView)) {
      items.push({
        key: '/projects',
        icon: <ProjectOutlined />,
        label: 'Dự án',
      });
    }

    if (hasPermission(PERMISSIONS.contractsView)) {
      items.push({
        key: '/contracts',
        icon: <ApartmentOutlined />,
        label: 'Hợp đồng',
      });
    }

    if (hasPermission(PERMISSIONS.paymentRequestsView)) {
      items.push({
        key: '/payment-requests',
        icon: <FileSearchOutlined />,
        label: 'Hồ sơ thanh toán',
      });
    }

    if (
      hasPermission(PERMISSIONS.paymentRequestsApprove) ||
      hasPermission(PERMISSIONS.paymentRequestsReject) ||
      hasPermission(PERMISSIONS.paymentRequestsReturn)
    ) {
      items.push({
        key: '/payment-requests/approvals',
        icon: <FileDoneOutlined />,
        label: 'Duyệt hồ sơ',
      });
    }

    if (hasPermission(PERMISSIONS.accountingConfirmationsView)) {
      items.push({
        key: '/accounting/confirmations',
        icon: <WalletOutlined />,
        label: 'Xác nhận thanh toán',
      });
    }

    if (hasPermission(PERMISSIONS.auditLogsView)) {
      items.push({
        key: '/audit-logs',
        icon: <AuditOutlined />,
        label: 'Audit Log',
      });
    }

    if (hasPermission(PERMISSIONS.approvalMatricesView)) {
      items.push({
        key: '/approval-matrices',
        icon: <SlidersOutlined />,
        label: 'Ma trận duyệt',
      });
    }

    if (hasPermission(PERMISSIONS.usersManage)) {
      items.push({
        key: '/users',
        icon: <UserOutlined />,
        label: 'Người dùng',
      });
    }

    if (hasPermission(PERMISSIONS.roleProfilesManage)) {
      items.push({
        key: '/role-permissions',
        icon: <SafetyCertificateOutlined />,
        label: 'Role & Phân quyền',
      });
    }

    return items;
  }, [hasPermission]);

  const activeKey = useMemo(() => {
    const matched = menuItems
      .map((item) => item?.key?.toString() ?? '')
      .filter((key) => key && location.pathname.startsWith(key))
      .sort((a, b) => b.length - a.length);

    return matched[0] ?? '/dashboard';
  }, [location.pathname, menuItems]);

  return (
    <Layout style={{ minHeight: '100vh', background: 'transparent' }}>
      <Sider
        breakpoint="lg"
        collapsedWidth="0"
        theme="light"
        style={{ borderRight: '1px solid #dbe4ee', background: '#f7fbff' }}
      >
        <div style={{ padding: 16, borderBottom: '1px solid #dbe4ee' }}>
          <Space align="start">
            <div className="app-brand-logo">
              <BuildOutlined />
            </div>
            <div className="app-brand-text">
              <strong>CPMS</strong>
              <span>Construction Payment Request</span>
            </div>
          </Space>
        </div>
        <Menu
          style={{ borderRight: 0, background: 'transparent' }}
          selectedKeys={[activeKey]}
          mode="inline"
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            background: '#fff',
            borderBottom: '1px solid #dbe4ee',
            padding: '0 18px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 16,
            position: 'relative',
            overflow: 'hidden',
          }}
        >
          <img
            src={skyline}
            alt="Skyline"
            style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover', opacity: 0.22 }}
          />
          <Space style={{ position: 'relative' }} size={12} wrap>
            <Typography.Title level={5} style={{ margin: 0 }}>
              Hệ thống quản lý hồ sơ thanh toán
            </Typography.Title>
            <DatabaseConnectionTag />
          </Space>
          <Space style={{ position: 'relative' }}>
            <Avatar icon={<UserOutlined />} />
            <Typography.Text style={{ maxWidth: 280 }} ellipsis>
              {user?.fullName} ({user?.roleProfileName ?? user?.role})
            </Typography.Text>
            <Button
              icon={<LogoutOutlined />}
              onClick={() => {
                void logout();
                navigate('/login', { replace: true });
              }}
            >
              Đăng xuất
            </Button>
          </Space>
        </Header>
        <Content style={{ margin: 16, overflow: 'auto' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
