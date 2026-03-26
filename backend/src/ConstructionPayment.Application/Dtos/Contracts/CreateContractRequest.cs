using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Application.Dtos.Contracts;

public class CreateContractRequest
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime SignedDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractType ContractType { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public bool IsActive { get; set; } = true;
}
