using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentRequestDto
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public string? ContractName { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal RequestedAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentRequestStatus CurrentStatus { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
}
