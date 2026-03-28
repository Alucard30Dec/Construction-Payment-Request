using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Authorization;

public static class RolePermissionDefaults
{
    private static readonly IReadOnlyDictionary<UserRole, string[]> Map = new Dictionary<UserRole, string[]>
    {
        [UserRole.Admin] = PermissionCatalog.All.Select(x => x.Code).ToArray(),

        [UserRole.Employee] = new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.SuppliersView,
            PermissionCodes.ProjectsView,
            PermissionCodes.ContractsView,
            PermissionCodes.ContractsAttachmentsManage,
            PermissionCodes.PaymentRequestsView,
            PermissionCodes.PaymentRequestsCreate,
            PermissionCodes.PaymentRequestsUpdate,
            PermissionCodes.PaymentRequestsDelete,
            PermissionCodes.PaymentRequestsSubmit,
            PermissionCodes.PaymentRequestsAttachmentsManage,
        },

        [UserRole.DepartmentManager] = new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.SuppliersView,
            PermissionCodes.SuppliersCreate,
            PermissionCodes.SuppliersUpdate,
            PermissionCodes.ProjectsView,
            PermissionCodes.ProjectsCreate,
            PermissionCodes.ProjectsUpdate,
            PermissionCodes.ContractsView,
            PermissionCodes.ContractsCreate,
            PermissionCodes.ContractsUpdate,
            PermissionCodes.ContractsAttachmentsManage,
            PermissionCodes.PaymentRequestsView,
            PermissionCodes.PaymentRequestsCreate,
            PermissionCodes.PaymentRequestsUpdate,
            PermissionCodes.PaymentRequestsSubmit,
            PermissionCodes.PaymentRequestsApprove,
            PermissionCodes.PaymentRequestsReject,
            PermissionCodes.PaymentRequestsReturn,
            PermissionCodes.PaymentRequestsAttachmentsManage,
            PermissionCodes.ApprovalMatricesView,
            PermissionCodes.AuditLogsView,
        },

        [UserRole.Director] = new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.SuppliersView,
            PermissionCodes.ProjectsView,
            PermissionCodes.ContractsView,
            PermissionCodes.PaymentRequestsView,
            PermissionCodes.PaymentRequestsApprove,
            PermissionCodes.PaymentRequestsReject,
            PermissionCodes.PaymentRequestsReturn,
            PermissionCodes.ApprovalMatricesView,
            PermissionCodes.AuditLogsView,
        },

        [UserRole.Accountant] = new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.SuppliersView,
            PermissionCodes.ProjectsView,
            PermissionCodes.ContractsView,
            PermissionCodes.PaymentRequestsView,
            PermissionCodes.PaymentRequestsReject,
            PermissionCodes.PaymentRequestsReturn,
            PermissionCodes.AccountingConfirmationsView,
            PermissionCodes.AccountingConfirmationsConfirm,
            PermissionCodes.AccountingInvoiceAttachmentsManage,
            PermissionCodes.AuditLogsView,
        },

        [UserRole.Viewer] = new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.SuppliersView,
            PermissionCodes.ProjectsView,
            PermissionCodes.ContractsView,
            PermissionCodes.PaymentRequestsView,
        },
    };

    public static IReadOnlyCollection<string> Get(UserRole role)
    {
        return Map.TryGetValue(role, out var permissionCodes)
            ? permissionCodes
            : Array.Empty<string>();
    }
}
