import { Suspense, lazy } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './components/ProtectedRoute';
import { PERMISSIONS } from './constants/permissions';

const LoginPage = lazy(() => import('./pages/auth/LoginPage').then((m) => ({ default: m.LoginPage })));
const AdminLayout = lazy(() => import('./layouts/AdminLayout').then((m) => ({ default: m.AdminLayout })));
const DashboardPage = lazy(() => import('./pages/dashboard/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const SupplierListPage = lazy(() => import('./pages/suppliers/SupplierListPage').then((m) => ({ default: m.SupplierListPage })));
const SupplierFormPage = lazy(() => import('./pages/suppliers/SupplierFormPage').then((m) => ({ default: m.SupplierFormPage })));
const ProjectListPage = lazy(() => import('./pages/projects/ProjectListPage').then((m) => ({ default: m.ProjectListPage })));
const ProjectFormPage = lazy(() => import('./pages/projects/ProjectFormPage').then((m) => ({ default: m.ProjectFormPage })));
const ContractListPage = lazy(() => import('./pages/contracts/ContractListPage').then((m) => ({ default: m.ContractListPage })));
const ContractFormPage = lazy(() => import('./pages/contracts/ContractFormPage').then((m) => ({ default: m.ContractFormPage })));
const ContractDetailPage = lazy(() => import('./pages/contracts/ContractDetailPage').then((m) => ({ default: m.ContractDetailPage })));
const PaymentRequestListPage = lazy(() => import('./pages/payment-requests/PaymentRequestListPage').then((m) => ({ default: m.PaymentRequestListPage })));
const PaymentRequestFormPage = lazy(() => import('./pages/payment-requests/PaymentRequestFormPage').then((m) => ({ default: m.PaymentRequestFormPage })));
const PaymentRequestDetailPage = lazy(() => import('./pages/payment-requests/PaymentRequestDetailPage').then((m) => ({ default: m.PaymentRequestDetailPage })));
const PaymentRequestApprovalPage = lazy(() => import('./pages/payment-requests/PaymentRequestApprovalPage').then((m) => ({ default: m.PaymentRequestApprovalPage })));
const PaymentConfirmationPage = lazy(() => import('./pages/accounting/PaymentConfirmationPage').then((m) => ({ default: m.PaymentConfirmationPage })));
const UserManagementPage = lazy(() => import('./pages/users/UserManagementPage').then((m) => ({ default: m.UserManagementPage })));
const AuditLogPage = lazy(() => import('./pages/audit/AuditLogPage').then((m) => ({ default: m.AuditLogPage })));
const ApprovalMatrixPage = lazy(() => import('./pages/approval-matrices/ApprovalMatrixPage').then((m) => ({ default: m.ApprovalMatrixPage })));
const RolePermissionMatrixPage = lazy(() => import('./pages/roles/RolePermissionMatrixPage').then((m) => ({ default: m.RolePermissionMatrixPage })));

function NotFoundPage() {
  return <div>Không tìm thấy trang</div>;
}

export default function App() {
  const routeLoadingFallback = (
    <div style={{ minHeight: '45vh', display: 'grid', placeItems: 'center' }}>
      Đang tải giao diện...
    </div>
  );

  return (
    <Suspense fallback={routeLoadingFallback}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute />}>
          <Route element={<AdminLayout />}>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.dashboardView]} />}>
              <Route path="/dashboard" element={<DashboardPage />} />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.suppliersView]} />}>
              <Route path="/suppliers" element={<SupplierListPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.suppliersCreate]} />}>
              <Route path="/suppliers/new" element={<SupplierFormPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.suppliersUpdate]} />}>
              <Route path="/suppliers/:id/edit" element={<SupplierFormPage />} />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.projectsView]} />}>
              <Route path="/projects" element={<ProjectListPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.projectsCreate]} />}>
              <Route path="/projects/new" element={<ProjectFormPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.projectsUpdate]} />}>
              <Route path="/projects/:id/edit" element={<ProjectFormPage />} />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.contractsView]} />}>
              <Route path="/contracts" element={<ContractListPage />} />
              <Route path="/contracts/:id" element={<ContractDetailPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.contractsCreate]} />}>
              <Route path="/contracts/new" element={<ContractFormPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.contractsUpdate]} />}>
              <Route path="/contracts/:id/edit" element={<ContractFormPage />} />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.paymentRequestsView]} />}>
              <Route path="/payment-requests" element={<PaymentRequestListPage />} />
              <Route path="/payment-requests/:id" element={<PaymentRequestDetailPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.paymentRequestsCreate]} />}>
              <Route path="/payment-requests/new" element={<PaymentRequestFormPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.paymentRequestsUpdate]} />}>
              <Route path="/payment-requests/:id/edit" element={<PaymentRequestFormPage />} />
            </Route>

            <Route
              element={
                <ProtectedRoute
                  permissions={[
                    PERMISSIONS.paymentRequestsApprove,
                    PERMISSIONS.paymentRequestsReject,
                    PERMISSIONS.paymentRequestsReturn,
                  ]}
                />
              }
            >
              <Route
                path="/payment-requests/approvals"
                element={<PaymentRequestApprovalPage />}
              />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.accountingConfirmationsView]} />}>
              <Route
                path="/accounting/confirmations"
                element={<PaymentConfirmationPage />}
              />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.approvalMatricesView]} />}>
              <Route path="/approval-matrices" element={<ApprovalMatrixPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.usersManage]} />}>
              <Route path="/users" element={<UserManagementPage />} />
            </Route>
            <Route element={<ProtectedRoute permissions={[PERMISSIONS.roleProfilesManage]} />}>
              <Route path="/role-permissions" element={<RolePermissionMatrixPage />} />
            </Route>

            <Route element={<ProtectedRoute permissions={[PERMISSIONS.auditLogsView]} />}>
              <Route path="/audit-logs" element={<AuditLogPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
