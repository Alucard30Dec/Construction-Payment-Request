import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminLayout } from './layouts/AdminLayout';
import { LoginPage } from './pages/auth/LoginPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { SupplierListPage } from './pages/suppliers/SupplierListPage';
import { SupplierFormPage } from './pages/suppliers/SupplierFormPage';
import { ProjectListPage } from './pages/projects/ProjectListPage';
import { ProjectFormPage } from './pages/projects/ProjectFormPage';
import { ContractListPage } from './pages/contracts/ContractListPage';
import { ContractFormPage } from './pages/contracts/ContractFormPage';
import { ContractDetailPage } from './pages/contracts/ContractDetailPage';
import { PaymentRequestListPage } from './pages/payment-requests/PaymentRequestListPage';
import { PaymentRequestFormPage } from './pages/payment-requests/PaymentRequestFormPage';
import { PaymentRequestDetailPage } from './pages/payment-requests/PaymentRequestDetailPage';
import { PaymentRequestApprovalPage } from './pages/payment-requests/PaymentRequestApprovalPage';
import { PaymentConfirmationPage } from './pages/accounting/PaymentConfirmationPage';
import { UserManagementPage } from './pages/users/UserManagementPage';
import { AuditLogPage } from './pages/audit/AuditLogPage';
import { ApprovalMatrixPage } from './pages/approval-matrices/ApprovalMatrixPage';
import { RolePermissionMatrixPage } from './pages/roles/RolePermissionMatrixPage';
import { PERMISSIONS } from './constants/permissions';

function NotFoundPage() {
  return <div>Không tìm thấy trang</div>;
}

export default function App() {
  return (
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
  );
}
