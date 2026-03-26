using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.ApprovalMatrices;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ApprovalMatricesController : ControllerBase
{
    private readonly IApprovalMatrixService _approvalMatrixService;

    public ApprovalMatricesController(IApprovalMatrixService approvalMatrixService)
    {
        _approvalMatrixService = approvalMatrixService;
    }

    [PermissionAuthorize(PermissionCodes.ApprovalMatricesView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ApprovalMatrixQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _approvalMatrixService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.ApprovalMatricesView)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var matrix = await _approvalMatrixService.GetByIdAsync(id, cancellationToken);
        return matrix is null ? NotFound() : Ok(matrix);
    }

    [PermissionAuthorize(PermissionCodes.ApprovalMatricesManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApprovalMatrixRequest request, CancellationToken cancellationToken)
    {
        var matrix = await _approvalMatrixService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = matrix.Id }, matrix);
    }

    [PermissionAuthorize(PermissionCodes.ApprovalMatricesManage)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApprovalMatrixRequest request, CancellationToken cancellationToken)
    {
        var matrix = await _approvalMatrixService.UpdateAsync(id, request, cancellationToken);
        return Ok(matrix);
    }

    [PermissionAuthorize(PermissionCodes.ApprovalMatricesManage)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _approvalMatrixService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
