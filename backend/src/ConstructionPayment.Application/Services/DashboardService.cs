using ConstructionPayment.Application.Dtos.Dashboard;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAppDbContext _dbContext;

    public DashboardService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var paymentRequests = _dbContext.PaymentRequests.AsNoTracking();
        var today = DateTime.UtcNow.Date;
        var dueSoonEndExclusive = today.AddDays(8);

        var aggregated = await paymentRequests
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalRequests = g.Count(),
                PendingApprovalCount = g.Count(x =>
                    x.CurrentStatus == PaymentRequestStatus.PendingDepartmentApproval ||
                    x.CurrentStatus == PaymentRequestStatus.PendingDirectorApproval ||
                    x.CurrentStatus == PaymentRequestStatus.PendingAccounting),
                PaidCount = g.Count(x => x.CurrentStatus == PaymentRequestStatus.Paid),
                OverdueCount = g.Count(x => x.DueDate < today && x.CurrentStatus != PaymentRequestStatus.Paid),
                DueSoonCount = g.Count(x =>
                    x.DueDate >= today &&
                    x.DueDate < dueSoonEndExclusive &&
                    x.CurrentStatus != PaymentRequestStatus.Paid),
                TotalRequestedAmount = g.Sum(x => (decimal?)x.RequestedAmount) ?? 0,
                PaidAmount = g.Where(x => x.CurrentStatus == PaymentRequestStatus.Paid)
                    .Sum(x => (decimal?)x.RequestedAmount) ?? 0,
                ApprovedCount = g.Count(x =>
                    x.CurrentStatus == PaymentRequestStatus.PendingAccounting ||
                    x.CurrentStatus == PaymentRequestStatus.Paid)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalRequests = aggregated?.TotalRequests ?? 0;
        var pendingApprovalCount = aggregated?.PendingApprovalCount ?? 0;
        var paidCount = aggregated?.PaidCount ?? 0;
        var overdueCount = aggregated?.OverdueCount ?? 0;
        var dueSoonCount = aggregated?.DueSoonCount ?? 0;
        var totalRequestedAmount = aggregated?.TotalRequestedAmount ?? 0;
        var paidAmount = aggregated?.PaidAmount ?? 0;
        var approvedCount = aggregated?.ApprovedCount ?? 0;

        var approvalRatePercent = totalRequests == 0
            ? 0
            : Math.Round(approvedCount * 100m / totalRequests, 2, MidpointRounding.AwayFromZero);

        var paidRatePercent = totalRequests == 0
            ? 0
            : Math.Round(paidCount * 100m / totalRequests, 2, MidpointRounding.AwayFromZero);

        var approvalDurations = await paymentRequests
            .Where(x => x.SubmittedAt.HasValue && x.ApprovedAt.HasValue)
            .Select(x => new { x.SubmittedAt, x.ApprovedAt })
            .ToListAsync(cancellationToken);

        var averageApprovalHours = approvalDurations.Count == 0
            ? 0
            : Math.Round(
                Convert.ToDecimal(approvalDurations.Average(x => (x.ApprovedAt!.Value - x.SubmittedAt!.Value).TotalHours)),
                2,
                MidpointRounding.AwayFromZero);

        var statusSummaries = await paymentRequests
            .GroupBy(x => x.CurrentStatus)
            .Select(g => new StatusSummaryDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);

        var monthlyAmountSummaries = await paymentRequests
            .Where(x => x.CreatedAt >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new MonthlyAmountSummaryDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(x => x.RequestedAmount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var amountByProject = await paymentRequests
            .GroupBy(x => new { x.ProjectId, ProjectName = x.Project != null ? x.Project.Name : string.Empty })
            .Select(g => new GroupAmountSummaryDto
            {
                Id = g.Key.ProjectId,
                Name = g.Key.ProjectName,
                TotalAmount = g.Sum(x => x.RequestedAmount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var amountBySupplier = await paymentRequests
            .GroupBy(x => new { x.SupplierId, SupplierName = x.Supplier != null ? x.Supplier.Name : string.Empty })
            .Select(g => new GroupAmountSummaryDto
            {
                Id = g.Key.SupplierId,
                Name = g.Key.SupplierName,
                TotalAmount = g.Sum(x => x.RequestedAmount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var overdueRequestsRaw = await paymentRequests
            .Where(x => x.DueDate < today && x.CurrentStatus != PaymentRequestStatus.Paid)
            .OrderBy(x => x.DueDate)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.RequestCode,
                x.Title,
                ProjectName = x.Project != null ? x.Project.Name : string.Empty,
                SupplierName = x.Supplier != null ? x.Supplier.Name : string.Empty,
                x.DueDate,
                x.RequestedAmount
            })
            .ToListAsync(cancellationToken);

        var topOverdueRequests = overdueRequestsRaw
            .Select(x => new DashboardOverdueItemDto
            {
                Id = x.Id,
                RequestCode = x.RequestCode,
                Title = x.Title,
                ProjectName = x.ProjectName,
                SupplierName = x.SupplierName,
                DueDate = x.DueDate,
                RequestedAmount = x.RequestedAmount,
                OverdueDays = Math.Max(1, (today - x.DueDate.Date).Days)
            })
            .ToArray();

        return new DashboardSummaryDto
        {
            TotalRequests = totalRequests,
            PendingApprovalCount = pendingApprovalCount,
            PaidCount = paidCount,
            OverdueCount = overdueCount,
            DueSoonCount = dueSoonCount,
            TotalRequestedAmount = totalRequestedAmount,
            PaidAmount = paidAmount,
            ApprovalRatePercent = approvalRatePercent,
            PaidRatePercent = paidRatePercent,
            AverageApprovalHours = averageApprovalHours,
            StatusSummaries = statusSummaries,
            MonthlyAmountSummaries = monthlyAmountSummaries,
            AmountByProject = amountByProject,
            AmountBySupplier = amountBySupplier,
            TopOverdueRequests = topOverdueRequests
        };
    }
}
