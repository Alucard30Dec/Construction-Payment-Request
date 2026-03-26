using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class Contract : AuditableEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime SignedDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractType ContractType { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public bool IsActive { get; set; } = true;

    public Supplier? Supplier { get; set; }
    public Project? Project { get; set; }
    public ICollection<PaymentRequestAttachment> Attachments { get; set; } = new List<PaymentRequestAttachment>();
    public ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
}
