using ConstructionPayment.Application.Common;

namespace ConstructionPayment.Application.Dtos.Suppliers;

public class SupplierQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
