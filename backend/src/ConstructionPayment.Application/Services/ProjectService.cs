using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Projects;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ProjectService(IAppDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProjectDto>> GetPagedAsync(ProjectQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(x => EF.Functions.Like(x.Code, pattern) || EF.Functions.Like(x.Name, pattern));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        return await query.ToPagedResultProjectedAsync(request.PageNumber, request.PageSize, x => new ProjectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Location = x.Location,
            Department = x.Department,
            ProjectManager = x.ProjectManager,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }, cancellationToken);
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueCodeAsync(request.Code, null, cancellationToken);

        var entity = new Project
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Location = request.Location?.Trim(),
            Department = request.Department?.Trim(),
            ProjectManager = request.ProjectManager?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Projects.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Create", nameof(Project), entity.Id.ToString(), null, entity, cancellationToken);

        return entity.ToDto();
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy dự án.");

        await EnsureUniqueCodeAsync(request.Code, id, cancellationToken);

        var oldValue = new
        {
            entity.Code,
            entity.Name,
            entity.Location,
            entity.Department,
            entity.ProjectManager,
            entity.IsActive
        };

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Location = request.Location?.Trim();
        entity.Department = request.Department?.Trim();
        entity.ProjectManager = request.ProjectManager?.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "Update",
            nameof(Project),
            entity.Id.ToString(),
            oldValue,
            entity,
            cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy dự án.");

        var hasDependency = await _dbContext.Contracts.AnyAsync(x => x.ProjectId == id, cancellationToken) ||
                            await _dbContext.PaymentRequests.AnyAsync(x => x.ProjectId == id, cancellationToken);

        if (hasDependency)
        {
            throw new AppException("Không thể xóa dự án đã phát sinh dữ liệu liên quan.");
        }

        _dbContext.Projects.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(_currentUser.UserId, "Delete", nameof(Project), id.ToString(), entity, null, cancellationToken);
    }

    private async Task EnsureUniqueCodeAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToLower();
        var duplicated = await _dbContext.Projects.AnyAsync(x =>
            x.Code.ToLower() == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicated)
        {
            throw new AppException("Mã dự án đã tồn tại.");
        }
    }
}
