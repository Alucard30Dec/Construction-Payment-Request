using ConstructionPayment.Application.Dtos.Attachments;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentRequestDetailDto
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
    public PaymentRequestStatus CurrentStatus { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<AttachmentDto> Attachments { get; set; } = Array.Empty<AttachmentDto>();
    public IReadOnlyCollection<PaymentRequestApprovalHistoryDto> ApprovalHistories { get; set; } = Array.Empty<PaymentRequestApprovalHistoryDto>();
    public PaymentConfirmationDto? PaymentConfirmation { get; set; }
}
