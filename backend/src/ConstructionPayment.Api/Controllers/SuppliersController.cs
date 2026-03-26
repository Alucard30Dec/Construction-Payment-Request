using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.Suppliers;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [PermissionAuthorize(PermissionCodes.SuppliersView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] SupplierQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _supplierService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.SuppliersView)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [PermissionAuthorize(PermissionCodes.SuppliersCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [PermissionAuthorize(PermissionCodes.SuppliersUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.UpdateAsync(id, request, cancellationToken);
        return Ok(supplier);
    }

    [PermissionAuthorize(PermissionCodes.SuppliersDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _supplierService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
