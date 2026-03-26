using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class PaymentRequestAttachment : AuditableEntity
{
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

    public PaymentRequest? PaymentRequest { get; set; }
    public Contract? Contract { get; set; }
    public PaymentConfirmation? PaymentConfirmation { get; set; }
    public AppUser? UploadedByUser { get; set; }
}
