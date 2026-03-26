using ConstructionPayment.Application.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.Users;

public class UserQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
