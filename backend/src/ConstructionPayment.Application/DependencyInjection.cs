using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConstructionPayment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IPaymentRequestService, PaymentRequestService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IApprovalMatrixService, ApprovalMatrixService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IRolePermissionResolver, RolePermissionService>();

        return services;
    }
}
