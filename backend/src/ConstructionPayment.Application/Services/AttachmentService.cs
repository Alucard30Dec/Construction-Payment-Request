using ConstructionPayment.Application.Dtos.Attachments;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Domain.Entities;
using ConstructionPayment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class AttachmentService : IAttachmentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx"
    };
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    private readonly IAppDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IRolePermissionResolver _rolePermissionResolver;

    public AttachmentService(
        IAppDbContext dbContext,
        IFileStorageService fileStorage,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IRolePermissionResolver rolePermissionResolver)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
        _auditService = auditService;
        _rolePermissionResolver = rolePermissionResolver;
    }

    public async Task<AttachmentDto> SavePaymentRequestAttachmentAsync(
        Guid paymentRequestId,
        string fileName,
        string contentType,
        long fileSize,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        await EnsureAnyPermissionAsync(
            new[]
            {
                PermissionCodes.PaymentRequestsAttachmentsManage,
                PermissionCodes.PaymentRequestsCreate,
                PermissionCodes.PaymentRequestsUpdate
            },
            cancellationToken);
        ValidateFile(fileName, fileSize);

        var paymentRequest = await _dbContext.PaymentRequests
            .FirstOrDefaultAsync(x => x.Id == paymentRequestId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (paymentRequest.CurrentStatus == PaymentRequestStatus.Paid)
        {
            throw new AppException("Hồ sơ đã thanh toán không thể chỉnh sửa file đính kèm.");
        }

        var attachment = await SaveInternalAsync(
            ownerType: AttachmentOwnerType.PaymentRequest,
            paymentRequestId: paymentRequestId,
            contractId: null,
            paymentConfirmationId: null,
            fileName: fileName,
            contentType: contentType,
            fileSize: fileSize,
            stream: stream,
            cancellationToken: cancellationToken);

        paymentRequest.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attachment;
    }

    public async Task<AttachmentDto> SaveContractAttachmentAsync(
        Guid contractId,
        string fileName,
        string contentType,
        long fileSize,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        await EnsureAnyPermissionAsync(
            new[]
            {
                PermissionCodes.ContractsAttachmentsManage,
                PermissionCodes.ContractsCreate,
                PermissionCodes.ContractsUpdate
            },
            cancellationToken);
        ValidateFile(fileName, fileSize);

        var contract = await _dbContext.Contracts
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hợp đồng.");

        var attachment = await SaveInternalAsync(
            ownerType: AttachmentOwnerType.Contract,
            paymentRequestId: null,
            contractId: contractId,
            paymentConfirmationId: null,
            fileName: fileName,
            contentType: contentType,
            fileSize: fileSize,
            stream: stream,
            cancellationToken: cancellationToken);

        contract.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attachment;
    }

    public async Task<AttachmentDto> SavePaymentConfirmationAttachmentAsync(
        Guid paymentRequestId,
        string fileName,
        string contentType,
        long fileSize,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync(PermissionCodes.AccountingInvoiceAttachmentsManage, cancellationToken);
        ValidateFile(fileName, fileSize);

        var paymentRequest = await _dbContext.PaymentRequests
            .Include(x => x.PaymentConfirmation)
            .FirstOrDefaultAsync(x => x.Id == paymentRequestId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hồ sơ thanh toán.");

        if (paymentRequest.CurrentStatus is not PaymentRequestStatus.PendingAccounting and not PaymentRequestStatus.Paid)
        {
            throw new AppException("Chỉ hồ sơ đang chờ kế toán hoặc đã thanh toán mới được tải chứng từ hóa đơn.");
        }

        if (paymentRequest.PaymentConfirmation is null)
        {
            paymentRequest.PaymentConfirmation = new PaymentConfirmation
            {
                PaymentRequestId = paymentRequest.Id,
                PaymentDate = DateTime.UtcNow,
                PaidAmount = 0,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        var attachment = await SaveInternalAsync(
            ownerType: AttachmentOwnerType.PaymentConfirmation,
            paymentRequestId: null,
            contractId: null,
            paymentConfirmationId: paymentRequest.PaymentConfirmation.Id,
            fileName: fileName,
            contentType: contentType,
            fileSize: fileSize,
            stream: stream,
            cancellationToken: cancellationToken);

        paymentRequest.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attachment;
    }

    public async Task<IReadOnlyCollection<AttachmentDto>> GetByPaymentRequestIdAsync(Guid paymentRequestId, CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync(PermissionCodes.PaymentRequestsView, cancellationToken);

        var items = await _dbContext.PaymentRequestAttachments
            .AsNoTracking()
            .Where(x => x.OwnerType == AttachmentOwnerType.PaymentRequest && x.PaymentRequestId == paymentRequestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(x => x.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<AttachmentDto>> GetByContractIdAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync(PermissionCodes.ContractsView, cancellationToken);

        var items = await _dbContext.PaymentRequestAttachments
            .AsNoTracking()
            .Where(x => x.OwnerType == AttachmentOwnerType.Contract && x.ContractId == contractId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(x => x.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<AttachmentDto>> GetByPaymentConfirmationAsync(Guid paymentRequestId, CancellationToken cancellationToken = default)
    {
        await EnsureAnyPermissionAsync(
            new[] { PermissionCodes.AccountingConfirmationsView, PermissionCodes.PaymentRequestsView },
            cancellationToken);

        var paymentConfirmationId = await _dbContext.PaymentConfirmations
            .AsNoTracking()
            .Where(x => x.PaymentRequestId == paymentRequestId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!paymentConfirmationId.HasValue)
        {
            return Array.Empty<AttachmentDto>();
        }

        var items = await _dbContext.PaymentRequestAttachments
            .AsNoTracking()
            .Where(x => x.OwnerType == AttachmentOwnerType.PaymentConfirmation && x.PaymentConfirmationId == paymentConfirmationId.Value)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(x => x.ToDto()).ToArray();
    }

    public async Task<(AttachmentDto metadata, Stream stream)> DownloadAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var entity = await GetAttachmentOrThrowAsync(attachmentId, cancellationToken);
        await EnsureReadPermissionForOwnerAsync(entity, cancellationToken);
        var stream = await _fileStorage.OpenReadAsync(entity.FilePath, cancellationToken);
        return (entity.ToDto(), stream);
    }

    public async Task<(AttachmentDto metadata, Stream stream)> PreviewAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var entity = await GetAttachmentOrThrowAsync(attachmentId, cancellationToken);
        await EnsureReadPermissionForOwnerAsync(entity, cancellationToken);
        var stream = await _fileStorage.OpenReadAsync(entity.FilePath, cancellationToken);
        return (entity.ToDto(), stream);
    }

    public async Task DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PaymentRequestAttachments
            .FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy file đính kèm.");

        await EnsureWritePermissionForOwnerAsync(entity, cancellationToken);

        if (entity.OwnerType == AttachmentOwnerType.PaymentRequest && entity.PaymentRequestId.HasValue)
        {
            var paymentRequest = await _dbContext.PaymentRequests
                .FirstOrDefaultAsync(x => x.Id == entity.PaymentRequestId.Value, cancellationToken);

            if (paymentRequest is not null && paymentRequest.CurrentStatus == PaymentRequestStatus.Paid)
            {
                throw new AppException("Hồ sơ đã thanh toán không thể xóa file đính kèm.");
            }
        }

        _dbContext.PaymentRequestAttachments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _fileStorage.DeleteFileAsync(entity.FilePath, cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "DeleteAttachment",
            nameof(PaymentRequestAttachment),
            entity.Id.ToString(),
            oldValue: ToAuditPayload(entity),
            newValue: null,
            cancellationToken);
    }

    private static void ValidateFile(string fileName, long fileSize)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new AppException("Định dạng file không được hỗ trợ. Chỉ chấp nhận: pdf, doc, docx, xls, xlsx.");
        }

        if (fileSize <= 0)
        {
            throw new AppException("File tải lên không hợp lệ.");
        }

        if (fileSize > MaxFileSizeBytes)
        {
            throw new AppException("File vượt quá dung lượng cho phép (tối đa 20 MB).");
        }
    }

    private async Task<AttachmentDto> SaveInternalAsync(
        AttachmentOwnerType ownerType,
        Guid? paymentRequestId,
        Guid? contractId,
        Guid? paymentConfirmationId,
        string fileName,
        string contentType,
        long fileSize,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var ownerCount =
            (paymentRequestId.HasValue ? 1 : 0) +
            (contractId.HasValue ? 1 : 0) +
            (paymentConfirmationId.HasValue ? 1 : 0);

        if (ownerCount != 1)
        {
            throw new AppException("Thông tin chủ sở hữu tệp đính kèm không hợp lệ.");
        }

        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("Không thể xác định người dùng hiện tại.");
        }

        var (storedFileName, filePath) = await _fileStorage.SaveFileAsync(stream, fileName, cancellationToken);
        var utcNow = DateTime.UtcNow;

        var entity = new PaymentRequestAttachment
        {
            OwnerType = ownerType,
            PaymentRequestId = paymentRequestId,
            ContractId = contractId,
            PaymentConfirmationId = paymentConfirmationId,
            FileName = fileName,
            StoredFileName = storedFileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedByUserId = _currentUser.UserId.Value,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.PaymentRequestAttachments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "UploadAttachment",
            nameof(PaymentRequestAttachment),
            entity.Id.ToString(),
            oldValue: null,
            newValue: ToAuditPayload(entity),
            cancellationToken);

        return entity.ToDto();
    }

    private async Task<PaymentRequestAttachment> GetAttachmentOrThrowAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentRequestAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy file đính kèm.");
    }

    private async Task EnsurePermissionAsync(string permissionCode, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("Không thể xác định người dùng hiện tại.");
        }

        var hasPermission = await _rolePermissionResolver.HasPermissionAsync(
            _currentUser.UserId.Value,
            permissionCode,
            cancellationToken);

        if (!hasPermission)
        {
            throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này.");
        }
    }

    private async Task EnsureAnyPermissionAsync(IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("Không thể xác định người dùng hiện tại.");
        }

        foreach (var permissionCode in permissionCodes)
        {
            var hasPermission = await _rolePermissionResolver.HasPermissionAsync(
                _currentUser.UserId.Value,
                permissionCode,
                cancellationToken);

            if (hasPermission)
            {
                return;
            }
        }

        throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này.");
    }

    private Task EnsureReadPermissionForOwnerAsync(PaymentRequestAttachment attachment, CancellationToken cancellationToken)
    {
        return attachment.OwnerType switch
        {
            AttachmentOwnerType.PaymentRequest => EnsurePermissionAsync(PermissionCodes.PaymentRequestsView, cancellationToken),
            AttachmentOwnerType.Contract => EnsurePermissionAsync(PermissionCodes.ContractsView, cancellationToken),
            AttachmentOwnerType.PaymentConfirmation => EnsureAnyPermissionAsync(
                new[] { PermissionCodes.AccountingConfirmationsView, PermissionCodes.PaymentRequestsView },
                cancellationToken),
            _ => throw new ForbiddenException("Loại tệp đính kèm không hợp lệ.")
        };
    }

    private Task EnsureWritePermissionForOwnerAsync(PaymentRequestAttachment attachment, CancellationToken cancellationToken)
    {
        return attachment.OwnerType switch
        {
            AttachmentOwnerType.PaymentRequest => EnsureAnyPermissionAsync(
                new[]
                {
                    PermissionCodes.PaymentRequestsAttachmentsManage,
                    PermissionCodes.PaymentRequestsCreate,
                    PermissionCodes.PaymentRequestsUpdate
                },
                cancellationToken),
            AttachmentOwnerType.Contract => EnsureAnyPermissionAsync(
                new[]
                {
                    PermissionCodes.ContractsAttachmentsManage,
                    PermissionCodes.ContractsCreate,
                    PermissionCodes.ContractsUpdate
                },
                cancellationToken),
            AttachmentOwnerType.PaymentConfirmation => EnsurePermissionAsync(PermissionCodes.AccountingInvoiceAttachmentsManage, cancellationToken),
            _ => throw new ForbiddenException("Loại tệp đính kèm không hợp lệ.")
        };
    }

    private static object ToAuditPayload(PaymentRequestAttachment entity)
    {
        return new
        {
            entity.Id,
            entity.OwnerType,
            entity.PaymentRequestId,
            entity.ContractId,
            entity.PaymentConfirmationId,
            entity.FileName,
            entity.StoredFileName,
            entity.ContentType,
            entity.FileSize,
            entity.UploadedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt
        };
    }
}
