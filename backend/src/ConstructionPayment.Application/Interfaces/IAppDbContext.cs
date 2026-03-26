using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<RoleProfile> RoleProfiles { get; }
    DbSet<RolePermissionGrant> RolePermissionGrants { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Project> Projects { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<PaymentRequest> PaymentRequests { get; }
    DbSet<PaymentRequestAttachment> PaymentRequestAttachments { get; }
    DbSet<PaymentRequestApprovalHistory> PaymentRequestApprovalHistories { get; }
    DbSet<ApprovalMatrix> ApprovalMatrices { get; }
    DbSet<PaymentConfirmation> PaymentConfirmations { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
