using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.AuditLogs;
using ConstructionPayment.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAppDbContext _dbContext;

    public AuditLogService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim().ToLower();
            query = query.Where(x => x.Action.ToLower() == action);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim().ToLower();
            query = query.Where(x => x.EntityName.ToLower() == entityName);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedAt <= toDate);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        return await query.ToPagedResultAsync(request.PageNumber, request.PageSize, x => new AuditLogDto
        {
            Id = x.Id,
            UserId = x.UserId,
            Username = x.User?.Username,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            OldValue = x.OldValue,
            NewValue = x.NewValue,
            CreatedAt = x.CreatedAt
        }, cancellationToken);
    }
}
