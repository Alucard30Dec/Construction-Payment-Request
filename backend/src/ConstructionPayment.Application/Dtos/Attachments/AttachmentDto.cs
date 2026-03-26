using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.Attachments;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public AttachmentOwnerType OwnerType { get; set; }
    public Guid? PaymentRequestId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? PaymentConfirmationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
