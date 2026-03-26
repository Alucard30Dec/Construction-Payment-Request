using ConstructionPayment.Application.Dtos.RolePermissions;

namespace ConstructionPayment.Application.Services;

public interface IRolePermissionService
{
    Task<IReadOnlyCollection<PermissionCatalogItemDto>> GetPermissionCatalogAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RoleProfileDto>> GetRoleProfilesAsync(CancellationToken cancellationToken = default);
    Task<RoleProfileDto?> GetRoleProfileByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleProfileDto> CreateRoleProfileAsync(CreateRoleProfileRequest request, CancellationToken cancellationToken = default);
    Task<RoleProfileDto> UpdateRoleProfileAsync(Guid id, UpdateRoleProfileRequest request, CancellationToken cancellationToken = default);
    Task<RoleProfileDto> SavePermissionsAsync(Guid roleProfileId, SaveRoleProfilePermissionsRequest request, CancellationToken cancellationToken = default);
    Task DeleteRoleProfileAsync(Guid roleProfileId, CancellationToken cancellationToken = default);
    Task<CurrentUserPermissionDto> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default);
}
