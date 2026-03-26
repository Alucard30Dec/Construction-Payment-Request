using ConstructionPayment.Application.Dtos.Attachments;

namespace ConstructionPayment.Application.Services;

public interface IAttachmentService
{
    Task<AttachmentDto> SavePaymentRequestAttachmentAsync(Guid paymentRequestId, string fileName, string contentType, long fileSize, Stream stream, CancellationToken cancellationToken = default);
    Task<AttachmentDto> SaveContractAttachmentAsync(Guid contractId, string fileName, string contentType, long fileSize, Stream stream, CancellationToken cancellationToken = default);
    Task<AttachmentDto> SavePaymentConfirmationAttachmentAsync(Guid paymentRequestId, string fileName, string contentType, long fileSize, Stream stream, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AttachmentDto>> GetByPaymentRequestIdAsync(Guid paymentRequestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AttachmentDto>> GetByContractIdAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AttachmentDto>> GetByPaymentConfirmationAsync(Guid paymentRequestId, CancellationToken cancellationToken = default);
    Task<(AttachmentDto metadata, Stream stream)> DownloadAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<(AttachmentDto metadata, Stream stream)> PreviewAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default);
}
