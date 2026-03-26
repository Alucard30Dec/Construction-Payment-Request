using ConstructionPayment.Domain.Common;

namespace ConstructionPayment.Domain.Entities;

public class ApprovalMatrix : AuditableEntity
{
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public bool RequireDirectorApproval { get; set; }
    public string? Department { get; set; }
    public Guid? ProjectId { get; set; }
    public bool IsActive { get; set; } = true;

    public Project? Project { get; set; }
}
