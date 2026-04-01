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
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            query = query.Where(x => EF.Functions.Like(x.Action, action));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim();
            query = query.Where(x => EF.Functions.Like(x.EntityName, entityName));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            var nextDate = request.ToDate.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < nextDate);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        return await query.ToPagedResultProjectedAsync(request.PageNumber, request.PageSize, x => new AuditLogDto
        {
            Id = x.Id,
            UserId = x.UserId,
            Username = x.User != null ? x.User.Username : null,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            OldValue = x.OldValue,
            NewValue = x.NewValue,
            CreatedAt = x.CreatedAt
        }, cancellationToken);
    }
}
