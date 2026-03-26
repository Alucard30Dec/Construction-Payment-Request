using ConstructionPayment.Application.Dtos.Auth;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, ICurrentUserService currentUser, IUserService userService)
    {
        _authService = authService;
        _currentUser = currentUser;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Đăng xuất thành công." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
