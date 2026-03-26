export const PERMISSIONS = {
  dashboardView: 'dashboard.view',

  suppliersView: 'suppliers.view',
  suppliersCreate: 'suppliers.create',
  suppliersUpdate: 'suppliers.update',
  suppliersDelete: 'suppliers.delete',

  projectsView: 'projects.view',
  projectsCreate: 'projects.create',
  projectsUpdate: 'projects.update',
  projectsDelete: 'projects.delete',

  contractsView: 'contracts.view',
  contractsCreate: 'contracts.create',
  contractsUpdate: 'contracts.update',
  contractsDelete: 'contracts.delete',
  contractsAttachmentsManage: 'contracts.attachments.manage',

  paymentRequestsView: 'payment_requests.view',
  paymentRequestsCreate: 'payment_requests.create',
  paymentRequestsUpdate: 'payment_requests.update',
  paymentRequestsDelete: 'payment_requests.delete',
  paymentRequestsSubmit: 'payment_requests.submit',
  paymentRequestsApprove: 'payment_requests.approve',
  paymentRequestsReject: 'payment_requests.reject',
  paymentRequestsReturn: 'payment_requests.return',
  paymentRequestsAttachmentsManage: 'payment_requests.attachments.manage',

  accountingConfirmationsView: 'accounting.confirmations.view',
  accountingConfirmationsConfirm: 'accounting.confirmations.confirm',
  accountingInvoiceAttachmentsManage: 'accounting.invoice_attachments.manage',

  approvalMatricesView: 'approval_matrices.view',
  approvalMatricesManage: 'approval_matrices.manage',

  auditLogsView: 'audit_logs.view',
  usersManage: 'users.manage',
  roleProfilesManage: 'role_profiles.manage',
} as const;
