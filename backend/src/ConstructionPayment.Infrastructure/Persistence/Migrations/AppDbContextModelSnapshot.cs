using System;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ConstructionPayment.Infrastructure.Persistence.Migrations
{
    [DbContextAttribute(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity<AppUser>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
                builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                builder.Property(x => x.Email).HasMaxLength(200);
                builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
                builder.Property(x => x.Role);
                builder.Property(x => x.Department).HasMaxLength(100);
                builder.Property(x => x.IsActive);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.Username).IsUnique();
                builder.ToTable("Users");
            });

            modelBuilder.Entity<Supplier>(builder =>
            {
                builder.Property(x => x.Id);
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
                builder.Property(x => x.IsActive);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => x.TaxCode).IsUnique();
                builder.ToTable("Suppliers");
            });

            modelBuilder.Entity<Project>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
                builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
                builder.Property(x => x.Location).HasMaxLength(500);
                builder.Property(x => x.Department).HasMaxLength(100);
                builder.Property(x => x.ProjectManager).HasMaxLength(200);
                builder.Property(x => x.IsActive);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.Code).IsUnique();
                builder.ToTable("Projects");
            });

            modelBuilder.Entity<Contract>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
                builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
                builder.Property(x => x.SupplierId);
                builder.Property(x => x.ProjectId);
                builder.Property(x => x.SignedDate);
                builder.Property(x => x.ContractValue).HasPrecision(18, 2);
                builder.Property(x => x.ContractType);
                builder.Property(x => x.Notes).HasMaxLength(1000);
                builder.Property(x => x.AttachmentPath).HasMaxLength(500);
                builder.Property(x => x.IsActive);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.ContractNumber).IsUnique();
                builder.HasIndex(x => x.ProjectId);
                builder.HasIndex(x => x.SupplierId);
                builder.ToTable("Contracts");
            });

            modelBuilder.Entity<PaymentRequest>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.RequestCode).HasMaxLength(100).IsRequired();
                builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
                builder.Property(x => x.ProjectId);
                builder.Property(x => x.SupplierId);
                builder.Property(x => x.ContractId);
                builder.Property(x => x.RequestType).HasMaxLength(100).IsRequired();
                builder.Property(x => x.InvoiceNumber).HasMaxLength(100).IsRequired();
                builder.Property(x => x.InvoiceDate);
                builder.Property(x => x.DueDate);
                builder.Property(x => x.Description).HasMaxLength(2000);
                builder.Property(x => x.AmountBeforeVat).HasPrecision(18, 2);
                builder.Property(x => x.VatRate).HasPrecision(5, 2);
                builder.Property(x => x.VatAmount).HasPrecision(18, 2);
                builder.Property(x => x.AmountAfterVat).HasPrecision(18, 2);
                builder.Property(x => x.AdvanceDeduction).HasPrecision(18, 2);
                builder.Property(x => x.RetentionAmount).HasPrecision(18, 2);
                builder.Property(x => x.OtherDeduction).HasPrecision(18, 2);
                builder.Property(x => x.RequestedAmount).HasPrecision(18, 2);
                builder.Property(x => x.PaymentMethod);
                builder.Property(x => x.CurrentStatus);
                builder.Property(x => x.CreatedByUserId);
                builder.Property(x => x.SubmittedAt);
                builder.Property(x => x.ApprovedAt);
                builder.Property(x => x.PaidAt);
                builder.Property(x => x.Notes).HasMaxLength(2000);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.RequestCode).IsUnique();
                builder.HasIndex(x => x.CurrentStatus);
                builder.HasIndex(x => new { x.SupplierId, x.InvoiceNumber });
                builder.HasIndex(x => x.ProjectId);
                builder.HasIndex(x => x.SupplierId);
                builder.HasIndex(x => x.ContractId);
                builder.HasIndex(x => x.CreatedByUserId);
                builder.ToTable("PaymentRequests");
            });

            modelBuilder.Entity<PaymentRequestAttachment>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.PaymentRequestId);
                builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
                builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
                builder.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
                builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
                builder.Property(x => x.FileSize);
                builder.Property(x => x.UploadedByUserId);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.PaymentRequestId);
                builder.HasIndex(x => x.UploadedByUserId);
                builder.ToTable("PaymentRequestAttachments");
            });

            modelBuilder.Entity<PaymentRequestApprovalHistory>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.PaymentRequestId);
                builder.Property(x => x.ApproverUserId);
                builder.Property(x => x.StepOrder);
                builder.Property(x => x.Action);
                builder.Property(x => x.ActionAt);
                builder.Property(x => x.Comment).HasMaxLength(2000);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.ApproverUserId);
                builder.HasIndex(x => new { x.PaymentRequestId, x.StepOrder, x.ActionAt })
                    .HasDatabaseName("IX_PRAppHist_Request_Step_ActionAt");
                builder.ToTable("PaymentRequestApprovalHistories");
            });

            modelBuilder.Entity<ApprovalMatrix>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.MinAmount).HasPrecision(18, 2);
                builder.Property(x => x.MaxAmount).HasPrecision(18, 2);
                builder.Property(x => x.RequireDirectorApproval);
                builder.Property(x => x.Department).HasMaxLength(100);
                builder.Property(x => x.ProjectId);
                builder.Property(x => x.IsActive);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.ProjectId);
                builder.HasIndex(x => new { x.IsActive, x.MinAmount, x.MaxAmount });
                builder.ToTable("ApprovalMatrices");
            });

            modelBuilder.Entity<PaymentConfirmation>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.PaymentRequestId);
                builder.Property(x => x.PaymentDate);
                builder.Property(x => x.PaymentReferenceNumber).HasMaxLength(100);
                builder.Property(x => x.BankTransactionNumber).HasMaxLength(100);
                builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
                builder.Property(x => x.AccountingNote).HasMaxLength(2000);
                builder.Property(x => x.PaymentStatus);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.PaymentRequestId).IsUnique();
                builder.ToTable("PaymentConfirmations");
            });

            modelBuilder.Entity<AuditLog>(builder =>
            {
                builder.Property(x => x.Id);
                builder.Property(x => x.UserId);
                builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
                builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
                builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
                builder.Property(x => x.OldValue);
                builder.Property(x => x.NewValue);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.UpdatedAt);
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.CreatedAt);
                builder.HasIndex(x => new { x.EntityName, x.EntityId });
                builder.HasIndex(x => x.UserId);
                builder.ToTable("AuditLogs");
            });

            modelBuilder.Entity<ApprovalMatrix>()
                .HasOne(x => x.Project)
                .WithMany(x => x.ApprovalMatrices)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Contract>()
                .HasOne(x => x.Project)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(x => x.Supplier)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentConfirmation>()
                .HasOne(x => x.PaymentRequest)
                .WithOne(x => x.PaymentConfirmation)
                .HasForeignKey<PaymentConfirmation>(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentRequest>()
                .HasOne(x => x.Contract)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentRequest>()
                .HasOne(x => x.Project)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentRequest>()
                .HasOne(x => x.Supplier)
                .WithMany(x => x.PaymentRequests)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentRequest>()
                .HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedPaymentRequests)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentRequestApprovalHistory>()
                .HasOne(x => x.PaymentRequest)
                .WithMany(x => x.ApprovalHistories)
                .HasForeignKey(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PRAppHist_PaymentRequest");

            modelBuilder.Entity<PaymentRequestApprovalHistory>()
                .HasOne(x => x.ApproverUser)
                .WithMany(x => x.ApprovalActions)
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentRequestAttachment>()
                .HasOne(x => x.PaymentRequest)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentRequestAttachment>()
                .HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
#pragma warning restore 612, 618
        }
    }
}
