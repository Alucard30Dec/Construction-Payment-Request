using ConstructionPayment.Domain.Enums;
using ConstructionPayment.Application.Dtos.Attachments;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentConfirmationDto
{
    public Guid Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentReferenceNumber { get; set; }
    public string? BankTransactionNumber { get; set; }
    public decimal PaidAmount { get; set; }
    public string? AccountingNote { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public IReadOnlyCollection<AttachmentDto> Attachments { get; set; } = Array.Empty<AttachmentDto>();
    public DateTime CreatedAt { get; set; }
}
