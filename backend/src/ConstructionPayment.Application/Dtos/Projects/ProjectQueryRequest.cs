using ConstructionPayment.Application.Common;

namespace ConstructionPayment.Application.Dtos.Projects;

public class ProjectQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
