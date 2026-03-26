namespace ConstructionPayment.Application.Dtos.Dashboard;

public class DashboardOverdueItemDto
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal RequestedAmount { get; set; }
    public int OverdueDays { get; set; }
}
