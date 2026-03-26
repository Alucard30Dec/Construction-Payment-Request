using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class PaymentConfirmation : AuditableEntity
{
    public Guid PaymentRequestId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentReferenceNumber { get; set; }
    public string? BankTransactionNumber { get; set; }
    public decimal PaidAmount { get; set; }
    public string? AccountingNote { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public PaymentRequest? PaymentRequest { get; set; }
    public ICollection<PaymentRequestAttachment> Attachments { get; set; } = new List<PaymentRequestAttachment>();
}
