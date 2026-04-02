using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.PaymentRequests;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using ConstructionPayment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class PaymentRequestService : IPaymentRequestService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public PaymentRequestService(IAppDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PaymentRequestDto>> GetPagedAsync(PaymentRequestQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PaymentRequests
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(x =>
                EF.Functions.Like(x.RequestCode, pattern) ||
                EF.Functions.Like(x.Title, pattern) ||
                EF.Functions.Like(x.InvoiceNumber, pattern) ||
                (x.Supplier != null && EF.Functions.Like(x.Supplier.Name, pattern)) ||
                (x.Project != null && EF.Functions.Like(x.Project.Name, pattern)));
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        if (request.SupplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.CurrentStatus == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;
            query = query.Where(x => x.CreatedAt >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedAt <= toDate);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        return await query.ToPagedResultProjectedAsync(request.PageNumber, request.PageSize, x => new PaymentRequestDto
        {
            Id = x.Id,
            RequestCode = x.RequestCode,
            Title = x.Title,
            ProjectId = x.ProjectId,
            ProjectName = x.Project != null ? x.Project.Name : string.Empty,
            SupplierId = x.SupplierId,
            SupplierName = x.Supplier != null ? x.Supplier.Name : string.Empty,
            ContractId = x.ContractId,
            ContractName = x.Contract != null ? x.Contract.Name : null,
            RequestType = x.RequestType,
            InvoiceNumber = x.InvoiceNumber,
            InvoiceDate = x.InvoiceDate,
            DueDate = x.DueDate,
            RequestedAmount = x.RequestedAmount,
            PaymentMethod = x.PaymentMethod,
            CurrentStatus = x.CurrentStatus,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUsername = x.CreatedByUser != null ? x.CreatedByUser.Username : string.Empty,
            SubmittedAt = x.SubmittedAt,
            ApprovedAt = x.ApprovedAt,
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            AttachmentCount = x.Attachments.Count()
        }, cancellationToken);
    }

    public async Task<PaymentRequestDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LoadForDetailAsync(id, cancellationToken);
        return entity?.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> CreateAsync(CreatePaymentRequestRequest request, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        if (role is UserRole.Viewer)
        {
            throw new ForbiddenException("Bạn không có quyền tạo hồ sơ thanh toán.");
        }

        await EnsureUniqueRequestCodeAsync(request.RequestCode, null, cancellationToken);
        await EnsureForeignKeysAsync(request.ProjectId, request.SupplierId, request.ContractId, cancellationToken);
        await EnsureNoDuplicateInvoiceAsync(request.SupplierId, request.InvoiceNumber, null, cancellationToken);

        var amounts = CalculateAmounts(
            request.AmountBeforeVat,
            request.VatRate,
            request.AdvanceDeduction,
            request.RetentionAmount,
            request.OtherDeduction);

        if (amounts.RequestedAmount <= 0)
        {
            throw new AppException("Số tiền đề nghị thanh toán phải lớn hơn 0.");
        }

        var entity = new PaymentRequest
        {
            RequestCode = request.RequestCode.Trim(),
            Title = request.Title.Trim(),
            ProjectId = request.ProjectId,
            SupplierId = request.SupplierId,
            ContractId = request.ContractId,
            RequestType = request.RequestType.Trim(),
            InvoiceNumber = request.InvoiceNumber.Trim(),
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            Description = request.Description?.Trim(),
            AmountBeforeVat = request.AmountBeforeVat,
            VatRate = request.VatRate,
            VatAmount = amounts.VatAmount,
            AmountAfterVat = amounts.AmountAfterVat,
            AdvanceDeduction = request.AdvanceDeduction,
            RetentionAmount = request.RetentionAmount,
            OtherDeduction = request.OtherDeduction,
            RequestedAmount = amounts.RequestedAmount,
            PaymentMethod = request.PaymentMethod,
            CurrentStatus = PaymentRequestStatus.Draft,
            CreatedByUserId = currentUserId,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PaymentRequests.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Create",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue: null,
            newValue: entity,
            cancellationToken);

        var created = await LoadForDetailOrThrowAsync(entity.Id, cancellationToken);
        return created.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> UpdateAsync(Guid id, UpdatePaymentRequestRequest request, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        EnsureEditable(entity, currentUserId, role);

        await EnsureForeignKeysAsync(request.ProjectId, request.SupplierId, request.ContractId, cancellationToken);
        await EnsureNoDuplicateInvoiceAsync(request.SupplierId, request.InvoiceNumber, id, cancellationToken);

        var amounts = CalculateAmounts(
            request.AmountBeforeVat,
            request.VatRate,
            request.AdvanceDeduction,
            request.RetentionAmount,
            request.OtherDeduction);

        if (amounts.RequestedAmount <= 0)
        {
            throw new AppException("Số tiền đề nghị thanh toán phải lớn hơn 0.");
        }

        var oldValue = new
        {
            entity.Title,
            entity.ProjectId,
            entity.SupplierId,
            entity.ContractId,
            entity.RequestType,
            entity.InvoiceNumber,
            entity.InvoiceDate,
            entity.DueDate,
            entity.Description,
            entity.AmountBeforeVat,
            entity.VatRate,
            entity.VatAmount,
            entity.AmountAfterVat,
            entity.AdvanceDeduction,
            entity.RetentionAmount,
            entity.OtherDeduction,
            entity.RequestedAmount,
            entity.PaymentMethod,
            entity.Notes,
            entity.CurrentStatus
        };

        entity.Title = request.Title.Trim();
        entity.ProjectId = request.ProjectId;
        entity.SupplierId = request.SupplierId;
        entity.ContractId = request.ContractId;
        entity.RequestType = request.RequestType.Trim();
        entity.InvoiceNumber = request.InvoiceNumber.Trim();
        entity.InvoiceDate = request.InvoiceDate;
        entity.DueDate = request.DueDate;
        entity.Description = request.Description?.Trim();
        entity.AmountBeforeVat = request.AmountBeforeVat;
        entity.VatRate = request.VatRate;
        entity.VatAmount = amounts.VatAmount;
        entity.AmountAfterVat = amounts.AmountAfterVat;
        entity.AdvanceDeduction = request.AdvanceDeduction;
        entity.RetentionAmount = request.RetentionAmount;
        entity.OtherDeduction = request.OtherDeduction;
        entity.RequestedAmount = amounts.RequestedAmount;
        entity.PaymentMethod = request.PaymentMethod;
        entity.Notes = request.Notes?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Update",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            entity,
            cancellationToken);

        var updated = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return updated.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests
            .Include(x => x.Attachments)
            .Include(x => x.ApprovalHistories)
            .Include(x => x.PaymentConfirmation)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        EnsureEditable(entity, currentUserId, role);

        if (entity.CurrentStatus is not PaymentRequestStatus.Draft and not PaymentRequestStatus.ReturnedForEdit and not PaymentRequestStatus.Rejected)
        {
            throw new AppException("Chỉ có thể xóa hồ sơ ở trạng thái Nháp, Trả chỉnh sửa hoặc Từ chối.");
        }

        _dbContext.PaymentRequests.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(currentUserId, "Delete", nameof(PaymentRequest), id.ToString(), entity, null, cancellationToken);
    }

    public async Task<PaymentRequestDetailDto> SubmitAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán không thể submit.");
        }

        if (entity.CreatedByUserId != currentUserId && role != UserRole.Admin)
        {
            throw new ForbiddenException("Chỉ người tạo hồ sơ hoặc admin mới được submit.");
        }

        if (entity.CurrentStatus is not PaymentRequestStatus.Draft and not PaymentRequestStatus.ReturnedForEdit)
        {
            throw new AppException("Chỉ có thể submit hồ sơ ở trạng thái Nháp hoặc Trả chỉnh sửa.");
        }

        EnsureRequestHasRequiredFields(entity);

        var requireDirector = await RequireDirectorApprovalAsync(entity, cancellationToken);

        var oldValue = new { entity.CurrentStatus, entity.SubmittedAt };

        entity.SubmittedAt = DateTime.UtcNow;
        entity.CurrentStatus = PaymentRequestStatus.PendingDepartmentApproval;
        entity.UpdatedAt = DateTime.UtcNow;

        _dbContext.PaymentRequestApprovalHistories.Add(new PaymentRequestApprovalHistory
        {
            PaymentRequestId = entity.Id,
            ApproverUserId = currentUserId,
            StepOrder = 0,
            Action = ApprovalAction.Submit,
            ActionAt = DateTime.UtcNow,
            Comment = request.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Submit",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            new { entity.CurrentStatus, entity.SubmittedAt, RequireDirector = requireDirector },
            cancellationToken);

        var loaded = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return loaded.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> ApproveAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán không thể duyệt.");
        }

        var oldValue = new { entity.CurrentStatus, entity.ApprovedAt };

        if (entity.CurrentStatus == PaymentRequestStatus.PendingDepartmentApproval)
        {
            if (role is not UserRole.DepartmentManager and not UserRole.Admin)
            {
                throw new ForbiddenException("Chỉ trưởng bộ phận hoặc admin được duyệt bước này.");
            }

            var requireDirector = await RequireDirectorApprovalAsync(entity, cancellationToken);
            entity.CurrentStatus = requireDirector
                ? PaymentRequestStatus.PendingDirectorApproval
                : PaymentRequestStatus.PendingAccounting;

            if (!requireDirector)
            {
                entity.ApprovedAt = DateTime.UtcNow;
            }

            AddApprovalHistory(entity.Id, currentUserId, 1, ApprovalAction.Approve, request.Comment);
        }
        else if (entity.CurrentStatus == PaymentRequestStatus.PendingDirectorApproval)
        {
            if (role is not UserRole.Director and not UserRole.Admin)
            {
                throw new ForbiddenException("Chỉ giám đốc hoặc admin được duyệt bước này.");
            }

            entity.CurrentStatus = PaymentRequestStatus.PendingAccounting;
            entity.ApprovedAt = DateTime.UtcNow;
            AddApprovalHistory(entity.Id, currentUserId, 2, ApprovalAction.Approve, request.Comment);
        }
        else
        {
            throw new AppException("Trạng thái hiện tại không hỗ trợ hành động duyệt.");
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Approve",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            new { entity.CurrentStatus, entity.ApprovedAt },
            cancellationToken);

        var loaded = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return loaded.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> RejectAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new AppException("Bắt buộc nhập lý do khi từ chối hồ sơ.");
        }

        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán không thể từ chối.");
        }

        var stepOrder = EnsureCanTakeWorkflowAction(entity.CurrentStatus, role);
        var oldValue = new { entity.CurrentStatus };

        entity.CurrentStatus = PaymentRequestStatus.Rejected;
        entity.UpdatedAt = DateTime.UtcNow;

        AddApprovalHistory(entity.Id, currentUserId, stepOrder, ApprovalAction.Reject, request.Comment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Reject",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            new { entity.CurrentStatus, Comment = request.Comment?.Trim() },
            cancellationToken);

        var loaded = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return loaded.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> ReturnForEditAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new AppException("Bắt buộc nhập lý do khi trả hồ sơ để chỉnh sửa.");
        }

        var (currentUserId, role) = GetCurrentUserOrThrow();

        var entity = await _dbContext.PaymentRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán không thể trả chỉnh sửa.");
        }

        var stepOrder = EnsureCanTakeWorkflowAction(entity.CurrentStatus, role);
        var oldValue = new { entity.CurrentStatus };

        entity.CurrentStatus = PaymentRequestStatus.ReturnedForEdit;
        entity.UpdatedAt = DateTime.UtcNow;

        AddApprovalHistory(entity.Id, currentUserId, stepOrder, ApprovalAction.ReturnForEdit, request.Comment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "ReturnForEdit",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            new { entity.CurrentStatus, Comment = request.Comment?.Trim() },
            cancellationToken);

        var loaded = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return loaded.ToDetailDto();
    }

    public async Task<PaymentRequestDetailDto> ConfirmPaymentAsync(Guid id, PaymentConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var (currentUserId, role) = GetCurrentUserOrThrow();

        if (role is not UserRole.Accountant and not UserRole.Admin)
        {
            throw new ForbiddenException("Chỉ kế toán hoặc admin được xác nhận thanh toán.");
        }

        var entity = await _dbContext.PaymentRequests
            .Include(x => x.PaymentConfirmation)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã được xác nhận thanh toán.");
        }

        if (entity.CurrentStatus != PaymentRequestStatus.PendingAccounting)
        {
            throw new AppException("Chỉ hồ sơ ở trạng thái Chờ kế toán xác nhận mới được thanh toán.");
        }

        if (request.PaymentStatus == PaymentStatus.Paid && request.PaidAmount < entity.RequestedAmount)
        {
            throw new AppException("Số tiền thanh toán phải lớn hơn hoặc bằng số tiền đề nghị khi trạng thái là Paid.");
        }

        var oldValue = new
        {
            entity.CurrentStatus,
            entity.PaidAt,
            entity.PaymentConfirmation
        };

        if (entity.PaymentConfirmation is null)
        {
            entity.PaymentConfirmation = new PaymentConfirmation
            {
                PaymentRequestId = entity.Id,
                PaymentDate = request.PaymentDate,
                PaymentReferenceNumber = request.PaymentReferenceNumber?.Trim(),
                BankTransactionNumber = request.BankTransactionNumber?.Trim(),
                PaidAmount = request.PaidAmount,
                AccountingNote = request.AccountingNote?.Trim(),
                PaymentStatus = request.PaymentStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            entity.PaymentConfirmation.PaymentDate = request.PaymentDate;
            entity.PaymentConfirmation.PaymentReferenceNumber = request.PaymentReferenceNumber?.Trim();
            entity.PaymentConfirmation.BankTransactionNumber = request.BankTransactionNumber?.Trim();
            entity.PaymentConfirmation.PaidAmount = request.PaidAmount;
            entity.PaymentConfirmation.AccountingNote = request.AccountingNote?.Trim();
            entity.PaymentConfirmation.PaymentStatus = request.PaymentStatus;
            entity.PaymentConfirmation.UpdatedAt = DateTime.UtcNow;
        }

        if (request.PaymentStatus == PaymentStatus.Paid)
        {
            entity.CurrentStatus = PaymentRequestStatus.Paid;
            entity.PaidAt = request.PaymentDate;
        }
        else
        {
            entity.CurrentStatus = PaymentRequestStatus.PendingAccounting;
            entity.PaidAt = null;
        }

        entity.UpdatedAt = DateTime.UtcNow;

        AddApprovalHistory(entity.Id, currentUserId, 3, ApprovalAction.ConfirmPayment, request.AccountingNote);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "ConfirmPayment",
            nameof(PaymentRequest),
            entity.Id.ToString(),
            oldValue,
            new
            {
                entity.CurrentStatus,
                entity.PaidAt,
                entity.PaymentConfirmation
            },
            cancellationToken);

        var loaded = await LoadForDetailOrThrowAsync(id, cancellationToken);
        return loaded.ToDetailDto();
    }

    private (Guid userId, UserRole role) GetCurrentUserOrThrow()
    {
        if (!_currentUser.UserId.HasValue || !_currentUser.Role.HasValue)
        {
            throw new ForbiddenException("Không thể xác định người dùng hiện tại.");
        }

        return (_currentUser.UserId.Value, _currentUser.Role.Value);
    }

    private void EnsureEditable(PaymentRequest entity, Guid currentUserId, UserRole role)
    {
        if (entity.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán nên bị khóa chỉnh sửa.");
        }

        if (entity.CurrentStatus is not PaymentRequestStatus.Draft and not PaymentRequestStatus.ReturnedForEdit and not PaymentRequestStatus.Rejected)
        {
            throw new AppException("Chỉ được chỉnh sửa hồ sơ ở trạng thái Nháp, Trả chỉnh sửa hoặc Từ chối.");
        }

        if (entity.CreatedByUserId != currentUserId && role != UserRole.Admin)
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa hồ sơ này.");
        }
    }

    private async Task EnsureUniqueRequestCodeAsync(string requestCode, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalizedCode = requestCode.Trim().ToLower();
        var exists = await _dbContext.PaymentRequests.AnyAsync(x =>
            x.RequestCode.ToLower() == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (exists)
        {
            throw new AppException("Mã hồ sơ thanh toán đã tồn tại.");
        }
    }

    private async Task EnsureForeignKeysAsync(Guid projectId, Guid supplierId, Guid? contractId, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            throw new AppException("Dự án không tồn tại.");
        }

        var supplierExists = await _dbContext.Suppliers.AnyAsync(x => x.Id == supplierId, cancellationToken);
        if (!supplierExists)
        {
            throw new AppException("Nhà cung cấp không tồn tại.");
        }

        if (!contractId.HasValue)
        {
            return;
        }

        var contract = await _dbContext.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == contractId.Value, cancellationToken)
            ?? throw new AppException("Hợp đồng không tồn tại.");

        if (contract.ProjectId != projectId)
        {
            throw new AppException("Hợp đồng đã chọn không thuộc dự án hiện tại.");
        }

        if (contract.SupplierId != supplierId)
        {
            throw new AppException("Hợp đồng đã chọn không thuộc nhà cung cấp hiện tại.");
        }
    }

    private async Task EnsureNoDuplicateInvoiceAsync(Guid supplierId, string invoiceNumber, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalizedInvoice = invoiceNumber.Trim().ToLower();

        var duplicated = await _dbContext.PaymentRequests.AnyAsync(x =>
            x.SupplierId == supplierId &&
            x.InvoiceNumber.ToLower() == normalizedInvoice &&
            x.CurrentStatus != PaymentRequestStatus.Cancelled &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicated)
        {
            throw new AppException("Số hóa đơn đã tồn tại cho nhà cung cấp này.");
        }
    }

    private static (decimal VatAmount, decimal AmountAfterVat, decimal RequestedAmount) CalculateAmounts(
        decimal amountBeforeVat,
        decimal vatRate,
        decimal advanceDeduction,
        decimal retentionAmount,
        decimal otherDeduction)
    {
        var vatAmount = Math.Round(amountBeforeVat * vatRate / 100m, 2, MidpointRounding.AwayFromZero);
        var amountAfterVat = Math.Round(amountBeforeVat + vatAmount, 2, MidpointRounding.AwayFromZero);
        var requestedAmount = Math.Round(amountAfterVat - advanceDeduction - retentionAmount - otherDeduction, 2, MidpointRounding.AwayFromZero);

        return (vatAmount, amountAfterVat, requestedAmount);
    }

    private void EnsureRequestHasRequiredFields(PaymentRequest entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Title) ||
            string.IsNullOrWhiteSpace(entity.RequestCode) ||
            string.IsNullOrWhiteSpace(entity.RequestType) ||
            string.IsNullOrWhiteSpace(entity.InvoiceNumber))
        {
            throw new AppException("Hồ sơ chưa đủ thông tin bắt buộc để submit.");
        }

        var amounts = CalculateAmounts(
            entity.AmountBeforeVat,
            entity.VatRate,
            entity.AdvanceDeduction,
            entity.RetentionAmount,
            entity.OtherDeduction);

        entity.VatAmount = amounts.VatAmount;
        entity.AmountAfterVat = amounts.AmountAfterVat;
        entity.RequestedAmount = amounts.RequestedAmount;

        if (entity.RequestedAmount <= 0)
        {
            throw new AppException("Số tiền đề nghị thanh toán phải lớn hơn 0.");
        }
    }

    private async Task<bool> RequireDirectorApprovalAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
    {
        var projectDepartment = paymentRequest.Project?.Department;

        if (string.IsNullOrWhiteSpace(projectDepartment))
        {
            projectDepartment = await _dbContext.Projects
                .Where(x => x.Id == paymentRequest.ProjectId)
                .Select(x => x.Department)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var department = projectDepartment?.Trim();

        var matrixQuery = _dbContext.ApprovalMatrices
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                paymentRequest.RequestedAmount >= x.MinAmount &&
                paymentRequest.RequestedAmount <= x.MaxAmount &&
                (!x.ProjectId.HasValue || x.ProjectId == paymentRequest.ProjectId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(department))
        {
            matrixQuery = matrixQuery.Where(x =>
                string.IsNullOrWhiteSpace(x.Department) ||
                EF.Functions.Like(x.Department!, department));
        }
        else
        {
            matrixQuery = matrixQuery.Where(x => string.IsNullOrWhiteSpace(x.Department));
        }

        var matrix = await matrixQuery
            .OrderByDescending(x => x.ProjectId.HasValue)
            .ThenByDescending(x => !string.IsNullOrWhiteSpace(x.Department))
            .ThenByDescending(x => x.MinAmount)
            .FirstOrDefaultAsync(cancellationToken);

        return matrix?.RequireDirectorApproval ?? false;
    }

    private int EnsureCanTakeWorkflowAction(PaymentRequestStatus currentStatus, UserRole role)
    {
        return currentStatus switch
        {
            PaymentRequestStatus.PendingDepartmentApproval when role is UserRole.DepartmentManager or UserRole.Admin => 1,
            PaymentRequestStatus.PendingDirectorApproval when role is UserRole.Director or UserRole.Admin => 2,
            PaymentRequestStatus.PendingAccounting when role is UserRole.Accountant or UserRole.Admin => 3,
            PaymentRequestStatus.PendingDepartmentApproval or PaymentRequestStatus.PendingDirectorApproval or PaymentRequestStatus.PendingAccounting
                => throw new ForbiddenException("Bạn không có quyền thực hiện hành động ở bước duyệt hiện tại."),
            _ => throw new AppException("Trạng thái hiện tại không hỗ trợ thao tác này.")
        };
    }

    private void AddApprovalHistory(Guid paymentRequestId, Guid approverUserId, int stepOrder, ApprovalAction action, string? comment)
    {
        _dbContext.PaymentRequestApprovalHistories.Add(new PaymentRequestApprovalHistory
        {
            PaymentRequestId = paymentRequestId,
            ApproverUserId = approverUserId,
            StepOrder = stepOrder,
            Action = action,
            ActionAt = DateTime.UtcNow,
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private async Task<PaymentRequest?> LoadForDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentRequests
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.Supplier)
            .Include(x => x.Contract)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Attachments)
            .Include(x => x.PaymentConfirmation!)
            .ThenInclude(x => x.Attachments)
            .Include(x => x.ApprovalHistories)
            .ThenInclude(x => x.ApproverUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<PaymentRequest> LoadForDetailOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        return await LoadForDetailAsync(id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");
    }
}
