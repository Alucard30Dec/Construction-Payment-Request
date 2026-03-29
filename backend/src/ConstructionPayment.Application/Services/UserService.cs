using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Users;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using ConstructionPayment.Domain.Entities;
using ConstructionPayment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IRolePermissionResolver _rolePermissionResolver;

    public UserService(
        IAppDbContext dbContext,
        IPasswordHasherService passwordHasher,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IRolePermissionResolver rolePermissionResolver)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _currentUser = currentUser;
        _rolePermissionResolver = rolePermissionResolver;
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(x => EF.Functions.Like(x.Username, pattern) || EF.Functions.Like(x.FullName, pattern));
        }

        if (request.Role.HasValue)
        {
            query = query.Where(x => x.Role == request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        return await query.ToPagedResultProjectedAsync(request.PageNumber, request.PageSize, x => new UserDto
        {
            Id = x.Id,
            Username = x.Username,
            FullName = x.FullName,
            Email = x.Email,
            Role = x.Role,
            RoleProfileId = x.RoleProfileId,
            RoleProfileCode = x.RoleProfile != null ? x.RoleProfile.Code : null,
            RoleProfileName = x.RoleProfile != null ? x.RoleProfile.Name : null,
            Department = x.Department,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        }, cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.RoleProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var permissionCodes = await _rolePermissionResolver.GetPermissionCodesAsync(user.Id, cancellationToken);
        return user.ToDto(permissionCodes);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var exists = await _dbContext.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower(), cancellationToken);
        if (exists)
        {
            throw new AppException("Tên đăng nhập đã tồn tại.");
        }

        var user = new AppUser
        {
            Username = username,
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            Role = request.Role,
            RoleProfileId = await ResolveRoleProfileIdAsync(request.Role, request.RoleProfileId, cancellationToken),
            Department = request.Department?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "Create",
            nameof(AppUser),
            user.Id.ToString(),
            oldValue: null,
            newValue: user,
            cancellationToken);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng.");

        var oldValue = new { user.FullName, user.Email, user.Role, user.RoleProfileId, user.Department, user.IsActive };

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim();
        user.Role = request.Role;
        user.RoleProfileId = await ResolveRoleProfileIdAsync(request.Role, request.RoleProfileId, cancellationToken);
        user.Department = request.Department?.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "Update",
            nameof(AppUser),
            user.Id.ToString(),
            oldValue,
            new { user.FullName, user.Email, user.Role, user.RoleProfileId, user.Department, user.IsActive },
            cancellationToken);

        return user.ToDto();
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUser.UserId,
            "ResetPassword",
            nameof(AppUser),
            user.Id.ToString(),
            oldValue: null,
            newValue: new { user.Id, user.Username },
            cancellationToken);
    }

    private async Task<Guid?> ResolveRoleProfileIdAsync(UserRole role, Guid? requestedRoleProfileId, CancellationToken cancellationToken)
    {
        if (requestedRoleProfileId.HasValue)
        {
            var exists = await _dbContext.RoleProfiles
                .AnyAsync(x => x.Id == requestedRoleProfileId.Value && x.IsActive, cancellationToken);

            if (!exists)
            {
                throw new AppException("Role profile không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            return requestedRoleProfileId;
        }

        var roleCode = role.ToString().ToLower();

        var defaultRoleProfileId = await _dbContext.RoleProfiles
            .Where(x => x.IsActive && x.Code.ToLower() == roleCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return defaultRoleProfileId;
    }
}
