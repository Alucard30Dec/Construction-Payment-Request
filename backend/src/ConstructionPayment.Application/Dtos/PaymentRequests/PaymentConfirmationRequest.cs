using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentConfirmationRequest
{
    public DateTime PaymentDate { get; set; }
    public string? PaymentReferenceNumber { get; set; }
    public string? BankTransactionNumber { get; set; }
    public decimal PaidAmount { get; set; }
    public string? AccountingNote { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
}
