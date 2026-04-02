import {
  ApartmentOutlined,
  AuditOutlined,
  BuildOutlined,
  DashboardOutlined,
  FileDoneOutlined,
  FileSearchOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuOutlined,
  MenuUnfoldOutlined,
  ProjectOutlined,
  SafetyCertificateOutlined,
  SlidersOutlined,
  TeamOutlined,
  UserOutlined,
  WalletOutlined,
} from '@ant-design/icons';
import { Avatar, Button, Drawer, Grid, Layout, Menu, Space, Typography } from 'antd';
import type { ItemType } from 'antd/es/menu/interface';
import { useEffect, useMemo, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { PERMISSIONS } from '../constants/permissions';
import skyline from '../assets/skyline.svg';
import { DatabaseConnectionTag } from '../components/DatabaseConnectionTag';

const { Header, Sider, Content } = Layout;

export function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout, hasPermission } = useAuth();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.lg;

  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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

  useEffect(() => {
    setMobileMenuOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    if (!isMobile) {
      setMobileMenuOpen(false);
    }
  }, [isMobile]);

  const menu = (
    <Menu
      className="app-shell__menu"
      style={{ borderInlineEnd: 0, background: 'transparent' }}
      selectedKeys={[activeKey]}
      mode="inline"
      items={menuItems}
      onClick={({ key }) => {
        navigate(key);
        setMobileMenuOpen(false);
      }}
    />
  );

  return (
    <Layout className="app-shell">
      {!isMobile && (
        <Sider
          className="app-shell__sider"
          theme="light"
          width={272}
          collapsedWidth={88}
          collapsible
          trigger={null}
          collapsed={collapsed}
        >
          <div className={`app-shell__brand ${collapsed ? 'app-shell__brand--compact' : ''}`}>
            <Space align="start">
              <div className="app-brand-logo">
                <BuildOutlined />
              </div>
              {!collapsed && (
                <div className="app-brand-text">
                  <strong>CPMS</strong>
                  <span>Construction Payment Request</span>
                </div>
              )}
            </Space>
          </div>
          {menu}
        </Sider>
      )}

      <Drawer
        className="app-shell__mobile-drawer"
        placement="left"
        width={304}
        open={mobileMenuOpen}
        onClose={() => setMobileMenuOpen(false)}
        title={
          <Space align="start">
            <div className="app-brand-logo">
              <BuildOutlined />
            </div>
            <div className="app-brand-text">
              <strong>CPMS</strong>
              <span>Construction Payment Request</span>
            </div>
          </Space>
        }
      >
        {menu}
      </Drawer>

      <Layout className="app-shell__content">
        <Header className="app-shell__header">
          <img
            src={skyline}
            alt="Skyline"
            style={{
              position: 'absolute',
              inset: 0,
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              opacity: 0.18,
            }}
          />

          <div className="app-shell__header-main">
            <Button
              className="app-shell__menu-trigger"
              type="text"
              icon={
                isMobile ? (
                  <MenuOutlined />
                ) : collapsed ? (
                  <MenuUnfoldOutlined />
                ) : (
                  <MenuFoldOutlined />
                )
              }
              onClick={() => {
                if (isMobile) {
                  setMobileMenuOpen(true);
                  return;
                }

                setCollapsed((prev) => !prev);
              }}
            />

            <div className="app-shell__title-group">
              <Typography.Title className="app-shell__title" level={5}>
                Hệ thống quản lý hồ sơ thanh toán
              </Typography.Title>
              <Typography.Text className="app-shell__subtitle">
                Tối ưu cho desktop và mobile, giữ nguyên toàn bộ luồng nghiệp vụ hiện tại.
              </Typography.Text>
              <DatabaseConnectionTag />
            </div>
          </div>

          <div className="app-shell__header-actions">
            <div className="app-shell__user">
              <Avatar icon={<UserOutlined />} />
              <div className="app-shell__user-meta">
                <Typography.Text strong ellipsis>
                  {user?.fullName}
                </Typography.Text>
                <Typography.Text type="secondary" ellipsis>
                  {user?.roleProfileName ?? user?.role}
                </Typography.Text>
              </div>
              <Button
                icon={<LogoutOutlined />}
                onClick={() => {
                  void logout();
                  navigate('/login', { replace: true });
                }}
              >
                Đăng xuất
              </Button>
            </div>
          </div>
        </Header>

        <Content className="app-shell__content-body">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
