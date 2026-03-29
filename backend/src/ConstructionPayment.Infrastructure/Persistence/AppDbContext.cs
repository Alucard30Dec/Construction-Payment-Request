using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RoleProfile> RoleProfiles => Set<RoleProfile>();
    public DbSet<RolePermissionGrant> RolePermissionGrants => Set<RolePermissionGrant>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();
    public DbSet<PaymentRequestAttachment> PaymentRequestAttachments => Set<PaymentRequestAttachment>();
    public DbSet<PaymentRequestApprovalHistory> PaymentRequestApprovalHistories => Set<PaymentRequestApprovalHistory>();
    public DbSet<ApprovalMatrix> ApprovalMatrices => Set<ApprovalMatrix>();
    public DbSet<PaymentConfirmation> PaymentConfirmations => Set<PaymentConfirmation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Department).HasMaxLength(100);
            builder.HasIndex(x => x.Username).IsUnique();
            builder.HasIndex(x => x.RoleProfileId);
            builder.HasIndex(x => new { x.Role, x.IsActive, x.CreatedAt });
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.RoleProfile)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoleProfile>(builder =>
        {
            builder.ToTable("RoleProfiles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<RolePermissionGrant>(builder =>
        {
            builder.ToTable("RolePermissionGrants");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PermissionCode).HasMaxLength(120).IsRequired();
            builder.HasIndex(x => new { x.RoleProfileId, x.PermissionCode }).IsUnique();

            builder.HasOne(x => x.RoleProfile)
                .WithMany(x => x.PermissionGrants)
                .HasForeignKey(x => x.RoleProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Supplier>(builder =>
        {
            builder.ToTable("Suppliers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.TaxCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.ContactPerson).HasMaxLength(200);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.BankAccountNumber).HasMaxLength(100);
            builder.Property(x => x.BankName).HasMaxLength(200);
            builder.Property(x => x.BankBranch).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.TaxCode).IsUnique();
        });

        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("Projects");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Location).HasMaxLength(500);
            builder.Property(x => x.Department).HasMaxLength(100);
            builder.Property(x => x.ProjectManager).HasMaxLength(200);
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Contract>(builder =>
        {
            builder.ToTable("Contracts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
            builder.Property(x => x.ContractValue).HasPrecision(18, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.AttachmentPath).HasMaxLength(500);
            builder.HasIndex(x => x.ContractNumber).IsUnique();
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => new { x.IsActive, x.CreatedAt });
            builder.HasIndex(x => new { x.SupplierId, x.CreatedAt });
            builder.HasIndex(x => new { x.ProjectId, x.CreatedAt });

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentRequest>(builder =>
        {
            builder.ToTable("PaymentRequests");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RequestCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
            builder.Property(x => x.RequestType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.InvoiceNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.Property(x => x.AmountBeforeVat).HasPrecision(18, 2);
            builder.Property(x => x.VatRate).HasPrecision(5, 2);
            builder.Property(x => x.VatAmount).HasPrecision(18, 2);
            builder.Property(x => x.AmountAfterVat).HasPrecision(18, 2);
            builder.Property(x => x.AdvanceDeduction).HasPrecision(18, 2);
            builder.Property(x => x.RetentionAmount).HasPrecision(18, 2);
            builder.Property(x => x.OtherDeduction).HasPrecision(18, 2);
            builder.Property(x => x.RequestedAmount).HasPrecision(18, 2);

            builder.HasIndex(x => x.RequestCode).IsUnique();
            builder.HasIndex(x => new { x.SupplierId, x.InvoiceNumber });
            builder.HasIndex(x => x.CurrentStatus);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => new { x.CurrentStatus, x.CreatedAt });
            builder.HasIndex(x => new { x.ProjectId, x.CreatedAt });
            builder.HasIndex(x => new { x.SupplierId, x.CreatedAt });

            builder.HasOne(x => x.Project)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Contract)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedPaymentRequests)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentRequestAttachment>(builder =>
        {
            builder.ToTable("PaymentRequestAttachments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OwnerType).IsRequired();
            builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();

            builder.HasOne(x => x.PaymentRequest)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Contract)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PaymentConfirmation)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.PaymentConfirmationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OwnerType);
            builder.HasIndex(x => x.ContractId);
            builder.HasIndex(x => x.PaymentConfirmationId);
        });

        modelBuilder.Entity<PaymentRequestApprovalHistory>(builder =>
        {
            builder.ToTable("PaymentRequestApprovalHistories");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Comment).HasMaxLength(2000);

            builder.HasOne(x => x.PaymentRequest)
                .WithMany(x => x.ApprovalHistories)
                .HasForeignKey(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PRAppHist_PaymentRequest");

            builder.HasOne(x => x.ApproverUser)
                .WithMany(x => x.ApprovalActions)
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PaymentRequestId, x.StepOrder, x.ActionAt })
                .HasDatabaseName("IX_PRAppHist_Request_Step_ActionAt");
        });

        modelBuilder.Entity<ApprovalMatrix>(builder =>
        {
            builder.ToTable("ApprovalMatrices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MinAmount).HasPrecision(18, 2);
            builder.Property(x => x.MaxAmount).HasPrecision(18, 2);
            builder.Property(x => x.Department).HasMaxLength(100);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ApprovalMatrices)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.IsActive, x.MinAmount, x.MaxAmount });
            builder.HasIndex(x => new { x.ProjectId, x.IsActive, x.MinAmount });
            builder.HasIndex(x => new { x.Department, x.IsActive, x.MinAmount });
        });

        modelBuilder.Entity<PaymentConfirmation>(builder =>
        {
            builder.ToTable("PaymentConfirmations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PaymentReferenceNumber).HasMaxLength(100);
            builder.Property(x => x.BankTransactionNumber).HasMaxLength(100);
            builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
            builder.Property(x => x.AccountingNote).HasMaxLength(2000);

            builder.HasOne(x => x.PaymentRequest)
                .WithOne(x => x.PaymentConfirmation)
                .HasForeignKey<PaymentConfirmation>(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PaymentRequestId).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
            builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => new { x.EntityName, x.EntityId });
            builder.HasIndex(x => new { x.UserId, x.CreatedAt });
            builder.HasIndex(x => new { x.Action, x.CreatedAt });
            builder.HasIndex(x => new { x.EntityName, x.CreatedAt });
        });

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
