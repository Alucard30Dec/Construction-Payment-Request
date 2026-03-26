namespace ConstructionPayment.Application.Dtos.Dashboard;

public class GroupAmountSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
