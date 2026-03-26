using ConstructionPayment.Domain.Common;
using ConstructionPayment.Domain.Enums;

namespace ConstructionPayment.Domain.Entities;

public class AppUser : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? RoleProfileId { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;

    public RoleProfile? RoleProfile { get; set; }
    public ICollection<PaymentRequest> CreatedPaymentRequests { get; set; } = new List<PaymentRequest>();
    public ICollection<PaymentRequestApprovalHistory> ApprovalActions { get; set; } = new List<PaymentRequestApprovalHistory>();
}
