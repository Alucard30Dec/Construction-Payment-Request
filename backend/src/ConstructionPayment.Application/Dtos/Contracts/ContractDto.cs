using ConstructionPayment.Domain.Enums;
using ConstructionPayment.Application.Dtos.Attachments;

namespace ConstructionPayment.Application.Dtos.Contracts;

public class ContractDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime SignedDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractType ContractType { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public int AttachmentCount { get; set; }
    public IReadOnlyCollection<AttachmentDto> Attachments { get; set; } = Array.Empty<AttachmentDto>();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
