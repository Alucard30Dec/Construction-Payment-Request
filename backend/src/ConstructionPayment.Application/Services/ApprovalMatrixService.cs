using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.ApprovalMatrices;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class ApprovalMatrixService : IApprovalMatrixService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ApprovalMatrixService(IAppDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ApprovalMatrixDto>> GetPagedAsync(ApprovalMatrixQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ApprovalMatrices
            .AsNoTracking()
            .Include(x => x.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            var department = request.Department.Trim().ToLower();
            query = query.Where(x => x.Department != null && x.Department.ToLower() == department);
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(x => x.MinAmount);

        return await query.ToPagedResultAsync(request.PageNumber, request.PageSize, x => x.ToDto(), cancellationToken);
    }

    public async Task<ApprovalMatrixDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ApprovalMatrices
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ApprovalMatrixDto> CreateAsync(CreateApprovalMatrixRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(request.ProjectId, cancellationToken);

        var entity = new ApprovalMatrix
        {
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            RequireDirectorApproval = request.RequireDirectorApproval,
            Department = request.Department?.Trim(),
            ProjectId = request.ProjectId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ApprovalMatrices.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Create", nameof(ApprovalMatrix), entity.Id.ToString(), null, entity, cancellationToken);

        var loaded = await _dbContext.ApprovalMatrices
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        return loaded.ToDto();
    }

    public async Task<ApprovalMatrixDto> UpdateAsync(Guid id, UpdateApprovalMatrixRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ApprovalMatrices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy cấu hình ma trận duyệt.");

        await EnsureProjectExistsAsync(request.ProjectId, cancellationToken);

        var oldValue = new
        {
            entity.MinAmount,
            entity.MaxAmount,
            entity.RequireDirectorApproval,
            entity.Department,
            entity.ProjectId,
            entity.IsActive
        };

        entity.MinAmount = request.MinAmount;
        entity.MaxAmount = request.MaxAmount;
        entity.RequireDirectorApproval = request.RequireDirectorApproval;
        entity.Department = request.Department?.Trim();
        entity.ProjectId = request.ProjectId;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Update", nameof(ApprovalMatrix), entity.Id.ToString(), oldValue, entity, cancellationToken);

        var loaded = await _dbContext.ApprovalMatrices
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        return loaded.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ApprovalMatrices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy cấu hình ma trận duyệt.");

        _dbContext.ApprovalMatrices.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Delete", nameof(ApprovalMatrix), id.ToString(), entity, null, cancellationToken);
    }

    private async Task EnsureProjectExistsAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        if (!projectId.HasValue)
        {
            return;
        }

        var exists = await _dbContext.Projects.AnyAsync(x => x.Id == projectId.Value, cancellationToken);
        if (!exists)
        {
            throw new AppException("Dự án không tồn tại.");
        }
    }
}
