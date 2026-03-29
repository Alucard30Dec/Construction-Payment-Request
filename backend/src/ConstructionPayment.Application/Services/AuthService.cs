using ConstructionPayment.Application.Dtos.Auth;
using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRolePermissionResolver _rolePermissionResolver;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRolePermissionResolver rolePermissionResolver)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _rolePermissionResolver = rolePermissionResolver;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var inputUsername = request.Username.Trim();

        var user = await _dbContext.Users
            .Include(x => x.RoleProfile)
            .FirstOrDefaultAsync(x => x.Username == inputUsername, cancellationToken);

        // Fallback để giữ tương thích đăng nhập không phân biệt hoa/thường.
        if (user is null)
        {
            var normalizedUsername = inputUsername.ToLowerInvariant();

            user = await _dbContext.Users
                .Include(x => x.RoleProfile)
                .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername, cancellationToken);
        }

        if (user is null || !user.IsActive)
        {
            throw new AppException("Tên đăng nhập hoặc mật khẩu không đúng.", 401);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new AppException("Tài khoản chưa được khởi tạo mật khẩu hợp lệ.", 401);
        }

        bool isValid;
        try
        {
            isValid = _passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password);
        }
        catch
        {
            throw new AppException("Thông tin mật khẩu tài khoản bị lỗi định dạng. Vui lòng reset dữ liệu seed.", 401);
        }

        if (!isValid)
        {
            throw new AppException("Tên đăng nhập hoặc mật khẩu không đúng.", 401);
        }

        string token;
        DateTime expiresAtUtc;

        try
        {
            (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);
        }
        catch (Exception ex)
        {
            throw new AppException($"Không thể tạo token đăng nhập: {ex.Message}", 500);
        }

        var permissionCodes = await _rolePermissionResolver.GetPermissionCodesAsync(user.Id, cancellationToken);

        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToDto(permissionCodes)
        };
    }
}
