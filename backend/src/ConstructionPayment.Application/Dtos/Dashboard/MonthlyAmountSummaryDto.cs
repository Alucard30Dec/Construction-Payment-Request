namespace ConstructionPayment.Application.Dtos.Dashboard;

public class MonthlyAmountSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
}
