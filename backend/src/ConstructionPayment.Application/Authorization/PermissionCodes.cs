namespace ConstructionPayment.Application.Authorization;

public static class PermissionCodes
{
    public const string DashboardView = "dashboard.view";

    public const string SuppliersView = "suppliers.view";
    public const string SuppliersCreate = "suppliers.create";
    public const string SuppliersUpdate = "suppliers.update";
    public const string SuppliersDelete = "suppliers.delete";

    public const string ProjectsView = "projects.view";
    public const string ProjectsCreate = "projects.create";
    public const string ProjectsUpdate = "projects.update";
    public const string ProjectsDelete = "projects.delete";

    public const string ContractsView = "contracts.view";
    public const string ContractsCreate = "contracts.create";
    public const string ContractsUpdate = "contracts.update";
    public const string ContractsDelete = "contracts.delete";
    public const string ContractsAttachmentsManage = "contracts.attachments.manage";

    public const string PaymentRequestsView = "payment_requests.view";
    public const string PaymentRequestsCreate = "payment_requests.create";
    public const string PaymentRequestsUpdate = "payment_requests.update";
    public const string PaymentRequestsDelete = "payment_requests.delete";
    public const string PaymentRequestsSubmit = "payment_requests.submit";
    public const string PaymentRequestsApprove = "payment_requests.approve";
    public const string PaymentRequestsReject = "payment_requests.reject";
    public const string PaymentRequestsReturn = "payment_requests.return";
    public const string PaymentRequestsAttachmentsManage = "payment_requests.attachments.manage";

    public const string AccountingConfirmationsView = "accounting.confirmations.view";
    public const string AccountingConfirmationsConfirm = "accounting.confirmations.confirm";
    public const string AccountingInvoiceAttachmentsManage = "accounting.invoice_attachments.manage";

    public const string ApprovalMatricesView = "approval_matrices.view";
    public const string ApprovalMatricesManage = "approval_matrices.manage";

    public const string AuditLogsView = "audit_logs.view";

    public const string UsersManage = "users.manage";
    public const string RoleProfilesManage = "role_profiles.manage";
}
