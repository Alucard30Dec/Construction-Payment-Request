using ConstructionPayment.Application.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.PaymentRequests;

public class PaymentRequestQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? SupplierId { get; set; }
    public PaymentRequestStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
