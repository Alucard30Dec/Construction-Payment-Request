namespace ConstructionPayment.Application.Dtos.Dashboard;

public class DashboardSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingApprovalCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
    public int DueSoonCount { get; set; }
    public decimal TotalRequestedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ApprovalRatePercent { get; set; }
    public decimal PaidRatePercent { get; set; }
    public decimal AverageApprovalHours { get; set; }
    public IReadOnlyCollection<StatusSummaryDto> StatusSummaries { get; set; } = Array.Empty<StatusSummaryDto>();
    public IReadOnlyCollection<MonthlyAmountSummaryDto> MonthlyAmountSummaries { get; set; } = Array.Empty<MonthlyAmountSummaryDto>();
    public IReadOnlyCollection<GroupAmountSummaryDto> AmountByProject { get; set; } = Array.Empty<GroupAmountSummaryDto>();
    public IReadOnlyCollection<GroupAmountSummaryDto> AmountBySupplier { get; set; } = Array.Empty<GroupAmountSummaryDto>();
    public IReadOnlyCollection<DashboardOverdueItemDto> TopOverdueRequests { get; set; } = Array.Empty<DashboardOverdueItemDto>();
}
