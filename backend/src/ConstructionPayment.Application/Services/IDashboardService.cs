using ConstructionPayment.Application.Dtos.Dashboard;

namespace ConstructionPayment.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
