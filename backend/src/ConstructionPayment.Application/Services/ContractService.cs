using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Contracts;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class ContractService : IContractService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ContractService(IAppDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ContractDto>> GetPagedAsync(ContractQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Contracts
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Project)
            .Include(x => x.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.ContractNumber.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword));
        }

        if (request.SupplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        return await query.ToPagedResultAsync(request.PageNumber, request.PageSize, x => x.ToDto(), cancellationToken);
    }

    public async Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Contracts
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Project)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueContractNumberAsync(request.ContractNumber, null, cancellationToken);
        await EnsureForeignKeysAsync(request.SupplierId, request.ProjectId, cancellationToken);

        var entity = new Contract
        {
            ContractNumber = request.ContractNumber.Trim(),
            Name = request.Name.Trim(),
            SupplierId = request.SupplierId,
            ProjectId = request.ProjectId,
            SignedDate = request.SignedDate,
            ContractValue = request.ContractValue,
            ContractType = request.ContractType,
            Notes = request.Notes?.Trim(),
            AttachmentPath = request.AttachmentPath?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Contracts.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        entity = await _dbContext.Contracts
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Project)
            .Include(x => x.Attachments)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Create", nameof(Contract), entity.Id.ToString(), null, entity, cancellationToken);

        return entity.ToDto();
    }

    public async Task<ContractDto> UpdateAsync(Guid id, UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Contracts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hợp đồng.");

        await EnsureUniqueContractNumberAsync(request.ContractNumber, id, cancellationToken);
        await EnsureForeignKeysAsync(request.SupplierId, request.ProjectId, cancellationToken);

        var oldValue = new
        {
            entity.ContractNumber,
            entity.Name,
            entity.SupplierId,
            entity.ProjectId,
            entity.SignedDate,
            entity.ContractValue,
            entity.ContractType,
            entity.Notes,
            entity.AttachmentPath,
            entity.IsActive
        };

        entity.ContractNumber = request.ContractNumber.Trim();
        entity.Name = request.Name.Trim();
        entity.SupplierId = request.SupplierId;
        entity.ProjectId = request.ProjectId;
        entity.SignedDate = request.SignedDate;
        entity.ContractValue = request.ContractValue;
        entity.ContractType = request.ContractType;
        entity.Notes = request.Notes?.Trim();
        entity.AttachmentPath = request.AttachmentPath?.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Update", nameof(Contract), entity.Id.ToString(), oldValue, entity, cancellationToken);

        var loaded = await _dbContext.Contracts
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Project)
            .Include(x => x.Attachments)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        return loaded.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Contracts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy hợp đồng.");

        var hasDependency = await _dbContext.PaymentRequests.AnyAsync(x => x.ContractId == id, cancellationToken);
        if (hasDependency)
        {
            throw new AppException("Không thể xóa hợp đồng đã phát sinh hồ sơ thanh toán.");
        }

        _dbContext.Contracts.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Delete", nameof(Contract), id.ToString(), entity, null, cancellationToken);
    }

    private async Task EnsureUniqueContractNumberAsync(string contractNumber, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = contractNumber.Trim().ToLower();
        var duplicated = await _dbContext.Contracts.AnyAsync(x =>
            x.ContractNumber.ToLower() == normalized &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicated)
        {
            throw new AppException("Số hợp đồng đã tồn tại.");
        }
    }

    private async Task EnsureForeignKeysAsync(Guid supplierId, Guid projectId, CancellationToken cancellationToken)
    {
        var supplierExists = await _dbContext.Suppliers.AnyAsync(x => x.Id == supplierId, cancellationToken);
        if (!supplierExists)
        {
            throw new AppException("Nhà cung cấp không tồn tại.");
        }

        var projectExists = await _dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            throw new AppException("Dự án không tồn tại.");
        }
    }
}
