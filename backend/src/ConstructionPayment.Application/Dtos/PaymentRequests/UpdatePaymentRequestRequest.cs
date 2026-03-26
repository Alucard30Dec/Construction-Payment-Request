using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class UpdatePaymentRequestRequest
{
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
    public decimal AdvanceDeduction { get; set; }
    public decimal RetentionAmount { get; set; }
    public decimal OtherDeduction { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}
