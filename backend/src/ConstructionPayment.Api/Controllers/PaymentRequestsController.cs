using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.PaymentRequests;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentRequestsController : ControllerBase
{
    private readonly IPaymentRequestService _paymentRequestService;

    public PaymentRequestsController(IPaymentRequestService paymentRequestService)
    {
        _paymentRequestService = paymentRequestService;
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] PaymentRequestQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentRequestService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsView)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.GetByIdAsync(id, cancellationToken);
        return paymentRequest is null ? NotFound() : Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequestRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = paymentRequest.Id }, paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentRequestRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.UpdateAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _paymentRequestService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsSubmit)]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.SubmitAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsApprove)]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.ApproveAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsReject)]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.RejectAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsReturn)]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnForEdit(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.ReturnForEditAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }

    [PermissionAuthorize(PermissionCodes.AccountingConfirmationsConfirm)]
    [HttpPost("{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] PaymentConfirmationRequest request, CancellationToken cancellationToken)
    {
        var paymentRequest = await _paymentRequestService.ConfirmPaymentAsync(id, request, cancellationToken);
        return Ok(paymentRequest);
    }
}
