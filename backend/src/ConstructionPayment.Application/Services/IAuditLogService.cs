using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.AuditLogs;

namespace ConstructionPayment.Application.Services;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryRequest request, CancellationToken cancellationToken = default);
}
