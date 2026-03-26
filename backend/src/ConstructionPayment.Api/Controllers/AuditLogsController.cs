using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.AuditLogs;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [PermissionAuthorize(PermissionCodes.AuditLogsView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] AuditLogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }
}
