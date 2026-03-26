namespace ConstructionPayment.Application.Dtos.ApprovalMatrices;

public class ApprovalMatrixDto
{
    public Guid Id { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public bool RequireDirectorApproval { get; set; }
    public string? Department { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
