using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentRequestApprovalHistoryDto
{
    public Guid Id { get; set; }
    public Guid ApproverUserId { get; set; }
    public string ApproverUsername { get; set; } = string.Empty;
    public string ApproverFullName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public ApprovalAction Action { get; set; }
    public DateTime ActionAt { get; set; }
    public string? Comment { get; set; }
}
