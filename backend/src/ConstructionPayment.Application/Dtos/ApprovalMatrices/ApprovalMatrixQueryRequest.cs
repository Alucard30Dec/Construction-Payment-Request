using ConstructionPayment.Application.Common;

namespace ConstructionPayment.Application.Dtos.ApprovalMatrices;

public class ApprovalMatrixQueryRequest : PaginationRequest
{
    public string? Department { get; set; }
    public Guid? ProjectId { get; set; }
    public bool? IsActive { get; set; }
}
