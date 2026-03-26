using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [PermissionAuthorize(PermissionCodes.PaymentRequestsView)]
    [HttpGet("payment-requests/{paymentRequestId:guid}/attachments")]
    public async Task<IActionResult> GetByPaymentRequestId(Guid paymentRequestId, CancellationToken cancellationToken)
    {
        var files = await _attachmentService.GetByPaymentRequestIdAsync(paymentRequestId, cancellationToken);
        return Ok(files);
    }

    [HttpPost("payment-requests/{paymentRequestId:guid}/attachments")]
    public async Task<IActionResult> UploadPaymentRequestAttachment(Guid paymentRequestId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file tải lên." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _attachmentService.SavePaymentRequestAttachmentAsync(
            paymentRequestId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.ContractsView)]
    [HttpGet("contracts/{contractId:guid}/attachments")]
    public async Task<IActionResult> GetByContractId(Guid contractId, CancellationToken cancellationToken)
    {
        var files = await _attachmentService.GetByContractIdAsync(contractId, cancellationToken);
        return Ok(files);
    }

    [HttpPost("contracts/{contractId:guid}/attachments")]
    public async Task<IActionResult> UploadContractAttachment(Guid contractId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file tải lên." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _attachmentService.SaveContractAttachmentAsync(
            contractId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.AccountingConfirmationsView)]
    [HttpGet("payment-requests/{paymentRequestId:guid}/payment-confirmation/attachments")]
    public async Task<IActionResult> GetByPaymentConfirmation(Guid paymentRequestId, CancellationToken cancellationToken)
    {
        var files = await _attachmentService.GetByPaymentConfirmationAsync(paymentRequestId, cancellationToken);
        return Ok(files);
    }

    [PermissionAuthorize(PermissionCodes.AccountingInvoiceAttachmentsManage)]
    [HttpPost("payment-requests/{paymentRequestId:guid}/payment-confirmation/attachments")]
    public async Task<IActionResult> UploadPaymentConfirmationAttachment(Guid paymentRequestId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file tải lên." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _attachmentService.SavePaymentConfirmationAttachmentAsync(
            paymentRequestId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid attachmentId, CancellationToken cancellationToken)
    {
        var (metadata, stream) = await _attachmentService.DownloadAsync(attachmentId, cancellationToken);
        return File(stream, metadata.ContentType, metadata.FileName);
    }

    [HttpGet("attachments/{attachmentId:guid}/preview")]
    public async Task<IActionResult> Preview(Guid attachmentId, CancellationToken cancellationToken)
    {
        var (metadata, stream) = await _attachmentService.PreviewAsync(attachmentId, cancellationToken);
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{metadata.FileName}\"");
        return File(stream, metadata.ContentType);
    }

    [HttpDelete("attachments/{attachmentId:guid}")]
    public async Task<IActionResult> Delete(Guid attachmentId, CancellationToken cancellationToken)
    {
        await _attachmentService.DeleteAsync(attachmentId, cancellationToken);
        return NoContent();
    }
}
