using ConstructionPayment.Application.Common;

namespace ConstructionPayment.Application.Dtos.AuditLogs;

public class AuditLogQueryRequest : PaginationRequest
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
