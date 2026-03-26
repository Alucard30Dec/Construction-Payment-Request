using ConstructionPayment.Application.Common;

namespace ConstructionPayment.Application.Dtos.Contracts;

public class ContractQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? ProjectId { get; set; }
    public bool? IsActive { get; set; }
}
