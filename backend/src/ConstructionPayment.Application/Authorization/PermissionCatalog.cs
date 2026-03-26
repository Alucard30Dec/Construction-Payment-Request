namespace ConstructionPayment.Application.Authorization;

public static class PermissionCatalog
{
    public static readonly IReadOnlyCollection<PermissionDefinition> All = new[]
    {
        new PermissionDefinition(PermissionCodes.DashboardView, "Xem dashboard", "Dashboard", "Cho phép xem số liệu tổng quan."),

        new PermissionDefinition(PermissionCodes.SuppliersView, "Xem nhà cung cấp", "Danh mục", "Xem danh sách và chi tiết nhà cung cấp."),
        new PermissionDefinition(PermissionCodes.SuppliersCreate, "Tạo nhà cung cấp", "Danh mục", "Tạo mới nhà cung cấp."),
        new PermissionDefinition(PermissionCodes.SuppliersUpdate, "Sửa nhà cung cấp", "Danh mục", "Cập nhật thông tin nhà cung cấp."),
        new PermissionDefinition(PermissionCodes.SuppliersDelete, "Xóa nhà cung cấp", "Danh mục", "Xóa nhà cung cấp chưa phát sinh dữ liệu phụ thuộc."),

        new PermissionDefinition(PermissionCodes.ProjectsView, "Xem dự án", "Danh mục", "Xem danh sách và chi tiết dự án."),
        new PermissionDefinition(PermissionCodes.ProjectsCreate, "Tạo dự án", "Danh mục", "Tạo mới dự án."),
        new PermissionDefinition(PermissionCodes.ProjectsUpdate, "Sửa dự án", "Danh mục", "Cập nhật thông tin dự án."),
        new PermissionDefinition(PermissionCodes.ProjectsDelete, "Xóa dự án", "Danh mục", "Xóa dự án chưa phát sinh dữ liệu phụ thuộc."),

        new PermissionDefinition(PermissionCodes.ContractsView, "Xem hợp đồng", "Hợp đồng", "Xem danh sách và chi tiết hợp đồng."),
        new PermissionDefinition(PermissionCodes.ContractsCreate, "Tạo hợp đồng", "Hợp đồng", "Tạo mới hợp đồng."),
        new PermissionDefinition(PermissionCodes.ContractsUpdate, "Sửa hợp đồng", "Hợp đồng", "Cập nhật thông tin hợp đồng."),
        new PermissionDefinition(PermissionCodes.ContractsDelete, "Xóa hợp đồng", "Hợp đồng", "Xóa hợp đồng chưa phát sinh hồ sơ thanh toán."),
        new PermissionDefinition(PermissionCodes.ContractsAttachmentsManage, "Quản lý file hợp đồng", "Hợp đồng", "Tải lên, xóa và xem trước file hợp đồng."),

        new PermissionDefinition(PermissionCodes.PaymentRequestsView, "Xem hồ sơ thanh toán", "Hồ sơ thanh toán", "Xem danh sách và chi tiết hồ sơ thanh toán."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsCreate, "Tạo hồ sơ thanh toán", "Hồ sơ thanh toán", "Tạo mới hồ sơ thanh toán."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsUpdate, "Sửa hồ sơ thanh toán", "Hồ sơ thanh toán", "Cập nhật hồ sơ ở trạng thái cho phép."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsDelete, "Xóa hồ sơ thanh toán", "Hồ sơ thanh toán", "Xóa hồ sơ ở trạng thái cho phép."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsSubmit, "Gửi duyệt hồ sơ", "Hồ sơ thanh toán", "Submit hồ sơ vào quy trình duyệt."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsApprove, "Duyệt hồ sơ", "Hồ sơ thanh toán", "Duyệt hồ sơ theo bước nghiệp vụ."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsReject, "Từ chối hồ sơ", "Hồ sơ thanh toán", "Từ chối hồ sơ theo bước nghiệp vụ."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsReturn, "Trả chỉnh sửa hồ sơ", "Hồ sơ thanh toán", "Trả hồ sơ về cho người tạo chỉnh sửa."),
        new PermissionDefinition(PermissionCodes.PaymentRequestsAttachmentsManage, "Quản lý file hồ sơ", "Hồ sơ thanh toán", "Tải lên, xóa và xem trước file hồ sơ thanh toán."),

        new PermissionDefinition(PermissionCodes.AccountingConfirmationsView, "Xem xác nhận thanh toán", "Kế toán", "Xem danh sách hồ sơ chờ kế toán."),
        new PermissionDefinition(PermissionCodes.AccountingConfirmationsConfirm, "Xác nhận thanh toán", "Kế toán", "Xác nhận thanh toán và cập nhật trạng thái chi trả."),
        new PermissionDefinition(PermissionCodes.AccountingInvoiceAttachmentsManage, "Quản lý file hóa đơn", "Kế toán", "Tải lên, xóa và xem trước chứng từ/hóa đơn kế toán."),

        new PermissionDefinition(PermissionCodes.ApprovalMatricesView, "Xem ma trận duyệt", "Quản trị", "Xem ma trận duyệt nghiệp vụ."),
        new PermissionDefinition(PermissionCodes.ApprovalMatricesManage, "Quản lý ma trận duyệt", "Quản trị", "Tạo/sửa/xóa ma trận duyệt."),

        new PermissionDefinition(PermissionCodes.AuditLogsView, "Xem nhật ký audit", "Quản trị", "Theo dõi lịch sử thao tác hệ thống."),
        new PermissionDefinition(PermissionCodes.UsersManage, "Quản lý người dùng", "Quản trị", "Tạo/sửa người dùng, reset mật khẩu."),
        new PermissionDefinition(PermissionCodes.RoleProfilesManage, "Quản lý role & phân quyền", "Quản trị", "Tạo role profile và cập nhật ma trận phân quyền."),
    };

    public static readonly HashSet<string> CodeSet =
        All.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
}
