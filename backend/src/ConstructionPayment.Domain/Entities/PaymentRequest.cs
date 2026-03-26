using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class PaymentRequest : AuditableEntity
{
    public string RequestCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? ContractId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Description { get; set; }
    public decimal AmountBeforeVat { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal AmountAfterVat { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal RetentionAmount { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal RequestedAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentRequestStatus CurrentStatus { get; set; } = PaymentRequestStatus.Draft;
    public Guid CreatedByUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }

    public Project? Project { get; set; }
    public Supplier? Supplier { get; set; }
    public Contract? Contract { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public PaymentConfirmation? PaymentConfirmation { get; set; }
    public ICollection<PaymentRequestAttachment> Attachments { get; set; } = new List<PaymentRequestAttachment>();
    public ICollection<PaymentRequestApprovalHistory> ApprovalHistories { get; set; } = new List<PaymentRequestApprovalHistory>();
}
