using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Users;

namespace ConstructionPayment.Application.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserQueryRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
