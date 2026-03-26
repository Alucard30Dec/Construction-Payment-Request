using ConstructionPayment.Application.Dtos.ApprovalMatrices;
using ConstructionPayment.Application.Dtos.Attachments;
using ConstructionPayment.Application.Dtos.Contracts;
using ConstructionPayment.Application.Dtos.PaymentRequests;
using ConstructionPayment.Application.Dtos.Projects;
using ConstructionPayment.Application.Dtos.Suppliers;
using ConstructionPayment.Application.Dtos.Users;
using ConstructionPayment.Domain.Entities;

namespace ConstructionPayment.Application.Mapping;

public static class DtoMapper
{
    public static UserDto ToDto(this AppUser entity, IReadOnlyCollection<string>? permissionCodes = null) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        FullName = entity.FullName,
        Email = entity.Email,
        Role = entity.Role,
        RoleProfileId = entity.RoleProfileId,
        RoleProfileCode = entity.RoleProfile?.Code,
        RoleProfileName = entity.RoleProfile?.Name,
        PermissionCodes = permissionCodes ?? Array.Empty<string>(),
        Department = entity.Department,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };

    public static SupplierDto ToDto(this Supplier entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        TaxCode = entity.TaxCode,
        Address = entity.Address,
        ContactPerson = entity.ContactPerson,
        Phone = entity.Phone,
        Email = entity.Email,
        BankAccountNumber = entity.BankAccountNumber,
        BankName = entity.BankName,
        BankBranch = entity.BankBranch,
        Notes = entity.Notes,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static ProjectDto ToDto(this Project entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Location = entity.Location,
        Department = entity.Department,
        ProjectManager = entity.ProjectManager,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static ContractDto ToDto(this Contract entity) => new()
    {
        Id = entity.Id,
        ContractNumber = entity.ContractNumber,
        Name = entity.Name,
        SupplierId = entity.SupplierId,
        SupplierName = entity.Supplier?.Name ?? string.Empty,
        ProjectId = entity.ProjectId,
        ProjectName = entity.Project?.Name ?? string.Empty,
        SignedDate = entity.SignedDate,
        ContractValue = entity.ContractValue,
        ContractType = entity.ContractType,
        Notes = entity.Notes,
        AttachmentPath = entity.AttachmentPath,
        AttachmentCount = entity.Attachments.Count,
        Attachments = entity.Attachments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.ToDto())
            .ToArray(),
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static AttachmentDto ToDto(this PaymentRequestAttachment entity) => new()
    {
        Id = entity.Id,
        OwnerType = entity.OwnerType,
        PaymentRequestId = entity.PaymentRequestId,
        ContractId = entity.ContractId,
        PaymentConfirmationId = entity.PaymentConfirmationId,
        FileName = entity.FileName,
        StoredFileName = entity.StoredFileName,
        FilePath = entity.FilePath,
        ContentType = entity.ContentType,
        FileSize = entity.FileSize,
        UploadedByUserId = entity.UploadedByUserId,
        CreatedAt = entity.CreatedAt
    };

    public static PaymentRequestDto ToListDto(this PaymentRequest entity) => new()
    {
        Id = entity.Id,
        RequestCode = entity.RequestCode,
        Title = entity.Title,
        ProjectId = entity.ProjectId,
        ProjectName = entity.Project?.Name ?? string.Empty,
        SupplierId = entity.SupplierId,
        SupplierName = entity.Supplier?.Name ?? string.Empty,
        ContractId = entity.ContractId,
        ContractName = entity.Contract?.Name,
        RequestType = entity.RequestType,
        InvoiceNumber = entity.InvoiceNumber,
        InvoiceDate = entity.InvoiceDate,
        DueDate = entity.DueDate,
        RequestedAmount = entity.RequestedAmount,
        PaymentMethod = entity.PaymentMethod,
        CurrentStatus = entity.CurrentStatus,
        CreatedByUserId = entity.CreatedByUserId,
        CreatedByUsername = entity.CreatedByUser?.Username ?? string.Empty,
        SubmittedAt = entity.SubmittedAt,
        ApprovedAt = entity.ApprovedAt,
        PaidAt = entity.PaidAt,
        CreatedAt = entity.CreatedAt,
        AttachmentCount = entity.Attachments.Count
    };

    public static PaymentRequestDetailDto ToDetailDto(this PaymentRequest entity) => new()
    {
        Id = entity.Id,
        RequestCode = entity.RequestCode,
        Title = entity.Title,
        ProjectId = entity.ProjectId,
        ProjectName = entity.Project?.Name ?? string.Empty,
        SupplierId = entity.SupplierId,
        SupplierName = entity.Supplier?.Name ?? string.Empty,
        ContractId = entity.ContractId,
        ContractName = entity.Contract?.Name,
        RequestType = entity.RequestType,
        InvoiceNumber = entity.InvoiceNumber,
        InvoiceDate = entity.InvoiceDate,
        DueDate = entity.DueDate,
        Description = entity.Description,
        AmountBeforeVat = entity.AmountBeforeVat,
        VatRate = entity.VatRate,
        VatAmount = entity.VatAmount,
        AmountAfterVat = entity.AmountAfterVat,
        AdvanceDeduction = entity.AdvanceDeduction,
        RetentionAmount = entity.RetentionAmount,
        OtherDeduction = entity.OtherDeduction,
        RequestedAmount = entity.RequestedAmount,
        PaymentMethod = entity.PaymentMethod,
        CurrentStatus = entity.CurrentStatus,
        CreatedByUserId = entity.CreatedByUserId,
        CreatedByUsername = entity.CreatedByUser?.Username ?? string.Empty,
        SubmittedAt = entity.SubmittedAt,
        ApprovedAt = entity.ApprovedAt,
        PaidAt = entity.PaidAt,
        Notes = entity.Notes,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        Attachments = entity.Attachments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.ToDto())
            .ToArray(),
        ApprovalHistories = entity.ApprovalHistories
            .OrderBy(h => h.StepOrder)
            .ThenBy(h => h.ActionAt)
            .Select(h => new PaymentRequestApprovalHistoryDto
            {
                Id = h.Id,
                ApproverUserId = h.ApproverUserId,
                ApproverUsername = h.ApproverUser?.Username ?? string.Empty,
                ApproverFullName = h.ApproverUser?.FullName ?? string.Empty,
                StepOrder = h.StepOrder,
                Action = h.Action,
                ActionAt = h.ActionAt,
                Comment = h.Comment
            }).ToArray(),
        PaymentConfirmation = entity.PaymentConfirmation is null
            ? null
            : new PaymentConfirmationDto
            {
                Id = entity.PaymentConfirmation.Id,
                PaymentDate = entity.PaymentConfirmation.PaymentDate,
                PaymentReferenceNumber = entity.PaymentConfirmation.PaymentReferenceNumber,
                BankTransactionNumber = entity.PaymentConfirmation.BankTransactionNumber,
                PaidAmount = entity.PaymentConfirmation.PaidAmount,
                AccountingNote = entity.PaymentConfirmation.AccountingNote,
                PaymentStatus = entity.PaymentConfirmation.PaymentStatus,
                Attachments = entity.PaymentConfirmation.Attachments
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.ToDto())
                    .ToArray(),
                CreatedAt = entity.PaymentConfirmation.CreatedAt
            }
    };

    public static ApprovalMatrixDto ToDto(this ApprovalMatrix entity) => new()
    {
        Id = entity.Id,
        MinAmount = entity.MinAmount,
        MaxAmount = entity.MaxAmount,
        RequireDirectorApproval = entity.RequireDirectorApproval,
        Department = entity.Department,
        ProjectId = entity.ProjectId,
        ProjectName = entity.Project?.Name,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
