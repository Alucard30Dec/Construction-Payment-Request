using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Suppliers;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public SupplierService(IAppDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(SupplierQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Code.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword) ||
                x.TaxCode.ToLower().Contains(keyword));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        return await query.ToPagedResultAsync(request.PageNumber, request.PageSize, x => x.ToDto(), cancellationToken);
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueAsync(request.Code, request.TaxCode, null, cancellationToken);

        var entity = new Supplier
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            TaxCode = request.TaxCode.Trim(),
            Address = request.Address?.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            BankAccountNumber = request.BankAccountNumber?.Trim(),
            BankName = request.BankName?.Trim(),
            BankBranch = request.BankBranch?.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Suppliers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Create", nameof(Supplier), entity.Id.ToString(), null, entity, cancellationToken);

        return entity.ToDto();
    }

    public async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy nhà cung cấp.");

        await EnsureUniqueAsync(request.Code, request.TaxCode, id, cancellationToken);

        var oldValue = new
        {
            entity.Code,
            entity.Name,
            entity.TaxCode,
            entity.Address,
            entity.ContactPerson,
            entity.Phone,
            entity.Email,
            entity.BankAccountNumber,
            entity.BankName,
            entity.BankBranch,
            entity.Notes,
            entity.IsActive
        };

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.TaxCode = request.TaxCode.Trim();
        entity.Address = request.Address?.Trim();
        entity.ContactPerson = request.ContactPerson?.Trim();
        entity.Phone = request.Phone?.Trim();
        entity.Email = request.Email?.Trim();
        entity.BankAccountNumber = request.BankAccountNumber?.Trim();
        entity.BankName = request.BankName?.Trim();
        entity.BankBranch = request.BankBranch?.Trim();
        entity.Notes = request.Notes?.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "Update",
            nameof(Supplier),
            entity.Id.ToString(),
            oldValue,
            entity,
            cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy nhà cung cấp.");

        var hasDependency = await _dbContext.Contracts.AnyAsync(x => x.SupplierId == id, cancellationToken) ||
                            await _dbContext.PaymentRequests.AnyAsync(x => x.SupplierId == id, cancellationToken);

        if (hasDependency)
        {
            throw new AppException("Không thể xóa nhà cung cấp đã phát sinh dữ liệu liên quan.");
        }

        _dbContext.Suppliers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Delete", nameof(Supplier), id.ToString(), entity, null, cancellationToken);
    }

    private async Task EnsureUniqueAsync(string code, string taxCode, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToLower();
        var normalizedTaxCode = taxCode.Trim().ToLower();

        var duplicatedCode = await _dbContext.Suppliers.AnyAsync(x =>
            x.Code.ToLower() == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicatedCode)
        {
            throw new AppException("Mã nhà cung cấp đã tồn tại.");
        }

        var duplicatedTax = await _dbContext.Suppliers.AnyAsync(x =>
            x.TaxCode.ToLower() == normalizedTaxCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicatedTax)
        {
            throw new AppException("Mã số thuế đã tồn tại.");
        }
    }
}
