using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.Contracts;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [PermissionAuthorize(PermissionCodes.ContractsView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ContractQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.ContractsView)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }

    [PermissionAuthorize(PermissionCodes.ContractsCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request, CancellationToken cancellationToken)
    {
        var contract = await _contractService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    [PermissionAuthorize(PermissionCodes.ContractsUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractRequest request, CancellationToken cancellationToken)
    {
        var contract = await _contractService.UpdateAsync(id, request, cancellationToken);
        return Ok(contract);
    }

    [PermissionAuthorize(PermissionCodes.ContractsDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _contractService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
