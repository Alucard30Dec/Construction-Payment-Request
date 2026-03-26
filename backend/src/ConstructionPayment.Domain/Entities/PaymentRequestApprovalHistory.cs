using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class PaymentRequestApprovalHistory : AuditableEntity
{
    public Guid PaymentRequestId { get; set; }
    public Guid ApproverUserId { get; set; }
    public int StepOrder { get; set; }
    public ApprovalAction Action { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }

    public PaymentRequest? PaymentRequest { get; set; }
    public AppUser? ApproverUser { get; set; }
}
