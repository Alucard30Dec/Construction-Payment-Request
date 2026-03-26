using System.Text.RegularExpressions;
using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.RolePermissions;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class RolePermissionService : IRolePermissionService, IRolePermissionResolver
{
    private static readonly Regex RoleCodeRegex = new("^[A-Za-z][A-Za-z0-9_]{2,49}$", RegexOptions.Compiled);

    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public RolePermissionService(
        IAppDbContext dbContext,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public Task<IReadOnlyCollection<PermissionCatalogItemDto>> GetPermissionCatalogAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<PermissionCatalogItemDto> result = PermissionCatalog.All
            .Select(x => new PermissionCatalogItemDto
            {
                Code = x.Code,
                Name = x.Name,
                Group = x.Group,
                Description = x.Description
            })
            .ToArray();

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyCollection<RoleProfileDto>> GetRoleProfilesAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _dbContext.RoleProfiles
            .AsNoTracking()
            .Include(x => x.PermissionGrants)
            .OrderBy(x => x.IsSystem ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var userCountByProfileId = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.RoleProfileId.HasValue)
            .GroupBy(x => x.RoleProfileId!.Value)
            .Select(g => new { RoleProfileId = g.Key, UserCount = g.Count() })
            .ToDictionaryAsync(x => x.RoleProfileId, x => x.UserCount, cancellationToken);

        return profiles
            .Select(x => x.ToDto(userCountByProfileId.TryGetValue(x.Id, out var count) ? count : 0))
            .ToArray();
    }

    public async Task<RoleProfileDto?> GetRoleProfileByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.RoleProfiles
            .AsNoTracking()
            .Include(x => x.PermissionGrants)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var userCount = await _dbContext.Users.CountAsync(x => x.RoleProfileId == id, cancellationToken);
        return profile.ToDto(userCount);
    }

    public async Task<RoleProfileDto> CreateRoleProfileAsync(CreateRoleProfileRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeRoleCode(request.Code);

        var duplicatedCode = await _dbContext.RoleProfiles
            .AnyAsync(x => x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken);

        if (duplicatedCode)
        {
            throw new AppException("Mã role profile đã tồn tại.");
        }

        var permissionCodes = request.GrantedPermissionCodes
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (request.CloneFromRoleProfileId.HasValue && permissionCodes.Count == 0)
        {
            var sourcePermissions = await _dbContext.RolePermissionGrants
                .AsNoTracking()
                .Where(x => x.RoleProfileId == request.CloneFromRoleProfileId.Value && x.IsAllowed)
                .Select(x => x.PermissionCode)
                .ToListAsync(cancellationToken);

            foreach (var code in sourcePermissions)
            {
                permissionCodes.Add(code);
            }
        }

        EnsurePermissionCodesAreValid(permissionCodes);

        var profile = new RoleProfile
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsSystem = false,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.RoleProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        SyncPermissionGrants(profile, permissionCodes, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "CreateRoleProfile",
            nameof(RoleProfile),
            profile.Id.ToString(),
            oldValue: null,
            new
            {
                profile.Code,
                profile.Name,
                profile.Description,
                profile.IsActive,
                GrantedPermissionCodes = permissionCodes.OrderBy(x => x).ToArray()
            },
            cancellationToken);

        return await GetRoleProfileByIdAsync(profile.Id, cancellationToken)
            ?? throw new AppException("Không thể tải role profile vừa tạo.");
    }

    public async Task<RoleProfileDto> UpdateRoleProfileAsync(Guid id, UpdateRoleProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.RoleProfiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy role profile.");

        var normalizedCode = NormalizeRoleCode(request.Code);

        var duplicatedCode = await _dbContext.RoleProfiles
            .AnyAsync(x => x.Id != id && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken);

        if (duplicatedCode)
        {
            throw new AppException("Mã role profile đã tồn tại.");
        }

        if (profile.IsSystem && !request.IsActive)
        {
            throw new AppException("Không thể vô hiệu hóa role profile hệ thống.");
        }

        var oldValue = new
        {
            profile.Code,
            profile.Name,
            profile.Description,
            profile.IsActive
        };

        if (!profile.IsSystem)
        {
            profile.Code = normalizedCode;
        }

        profile.Name = request.Name.Trim();
        profile.Description = request.Description?.Trim();
        profile.IsActive = request.IsActive;
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "UpdateRoleProfile",
            nameof(RoleProfile),
            profile.Id.ToString(),
            oldValue,
            new
            {
                profile.Code,
                profile.Name,
                profile.Description,
                profile.IsActive
            },
            cancellationToken);

        return await GetRoleProfileByIdAsync(profile.Id, cancellationToken)
            ?? throw new AppException("Không thể tải role profile sau khi cập nhật.");
    }

    public async Task<RoleProfileDto> SavePermissionsAsync(Guid roleProfileId, SaveRoleProfilePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.RoleProfiles
            .Include(x => x.PermissionGrants)
            .FirstOrDefaultAsync(x => x.Id == roleProfileId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy role profile.");

        if (!profile.IsActive)
        {
            throw new AppException("Role profile đang bị vô hiệu hóa, không thể cập nhật phân quyền.");
        }

        var permissionCodes = request.GrantedPermissionCodes
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        EnsurePermissionCodesAreValid(permissionCodes);

        var oldAllowedCodes = profile.PermissionGrants
            .Where(x => x.IsAllowed)
            .Select(x => x.PermissionCode)
            .OrderBy(x => x)
            .ToArray();

        SyncPermissionGrants(profile, permissionCodes, DateTime.UtcNow);
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "UpdateRolePermissions",
            nameof(RoleProfile),
            profile.Id.ToString(),
            oldValue: oldAllowedCodes,
            newValue: permissionCodes.OrderBy(x => x).ToArray(),
            cancellationToken);

        return await GetRoleProfileByIdAsync(profile.Id, cancellationToken)
            ?? throw new AppException("Không thể tải role profile sau khi cập nhật quyền.");
    }

    public async Task DeleteRoleProfileAsync(Guid roleProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.RoleProfiles
            .Include(x => x.Users)
            .Include(x => x.PermissionGrants)
            .FirstOrDefaultAsync(x => x.Id == roleProfileId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy role profile.");

        if (profile.IsSystem)
        {
            throw new AppException("Không thể xóa role profile hệ thống.");
        }

        if (profile.Users.Count > 0)
        {
            throw new AppException("Role profile đang được gán cho người dùng, không thể xóa.");
        }

        _dbContext.RolePermissionGrants.RemoveRange(profile.PermissionGrants);
        _dbContext.RoleProfiles.Remove(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "DeleteRoleProfile",
            nameof(RoleProfile),
            roleProfileId.ToString(),
            oldValue: new { profile.Code, profile.Name },
            newValue: null,
            cancellationToken);
    }

    public async Task<CurrentUserPermissionDto> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return new CurrentUserPermissionDto();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.RoleProfile)
            .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new CurrentUserPermissionDto();
        }

        var permissionCodes = await ResolvePermissionCodesAsync(user, cancellationToken);

        return new CurrentUserPermissionDto
        {
            RoleProfileId = user.RoleProfileId,
            RoleProfileCode = user.RoleProfile?.Code,
            RoleProfileName = user.RoleProfile?.Name,
            PermissionCodes = permissionCodes
        };
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.RoleProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return false;
        }

        var permissionCodes = await ResolvePermissionCodesAsync(user, cancellationToken);
        return permissionCodes.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.RoleProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Array.Empty<string>();
        }

        return await ResolvePermissionCodesAsync(user, cancellationToken);
    }

    private static string NormalizeRoleCode(string code)
    {
        var normalized = code.Trim();
        if (!RoleCodeRegex.IsMatch(normalized))
        {
            throw new AppException("Mã role profile không hợp lệ. Chỉ chấp nhận chữ, số, dấu gạch dưới và tối thiểu 3 ký tự.");
        }

        return normalized;
    }

    private static void EnsurePermissionCodesAreValid(IEnumerable<string> permissionCodes)
    {
        var invalidCodes = permissionCodes
            .Where(x => !PermissionCatalog.CodeSet.Contains(x))
            .OrderBy(x => x)
            .ToArray();

        if (invalidCodes.Length > 0)
        {
            throw new AppException($"Danh sách quyền không hợp lệ: {string.Join(", ", invalidCodes)}.");
        }
    }

    private static void SyncPermissionGrants(RoleProfile profile, IReadOnlyCollection<string> allowedCodes, DateTime utcNow)
    {
        foreach (var permission in PermissionCatalog.All)
        {
            var grant = profile.PermissionGrants.FirstOrDefault(x =>
                x.PermissionCode.Equals(permission.Code, StringComparison.OrdinalIgnoreCase));

            var isAllowed = allowedCodes.Contains(permission.Code, StringComparer.OrdinalIgnoreCase);

            if (grant is null)
            {
                profile.PermissionGrants.Add(new RolePermissionGrant
                {
                    RoleProfileId = profile.Id,
                    PermissionCode = permission.Code,
                    IsAllowed = isAllowed,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow
                });
                continue;
            }

            grant.IsAllowed = isAllowed;
            grant.UpdatedAt = utcNow;
        }
    }

    private async Task<IReadOnlyCollection<string>> ResolvePermissionCodesAsync(AppUser user, CancellationToken cancellationToken)
    {
        if (user.RoleProfileId.HasValue && user.RoleProfile?.IsActive == true)
        {
            var directRolePermissions = await _dbContext.RolePermissionGrants
                .AsNoTracking()
                .Where(x => x.RoleProfileId == user.RoleProfileId.Value && x.IsAllowed)
                .Select(x => x.PermissionCode)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            return directRolePermissions;
        }

        var fallbackCode = user.Role.ToString();

        var roleProfilePermissions = await _dbContext.RoleProfiles
            .AsNoTracking()
            .Where(x => x.Code.ToLower() == fallbackCode.ToLower() && x.IsActive)
            .SelectMany(x => x.PermissionGrants.Where(g => g.IsAllowed).Select(g => g.PermissionCode))
            .Distinct()
            .ToArrayAsync(cancellationToken);

        if (roleProfilePermissions.Length > 0)
        {
            return roleProfilePermissions;
        }

        return RolePermissionDefaults.Get(user.Role).ToArray();
    }
}

internal static class RoleProfileMapperExtensions
{
    public static RoleProfileDto ToDto(this RoleProfile entity, int userCount) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Description = entity.Description,
        IsSystem = entity.IsSystem,
        IsActive = entity.IsActive,
        UserCount = userCount,
        GrantedPermissionCodes = entity.PermissionGrants
            .Where(x => x.IsAllowed)
            .Select(x => x.PermissionCode)
            .OrderBy(x => x)
            .ToArray()
    };
}
