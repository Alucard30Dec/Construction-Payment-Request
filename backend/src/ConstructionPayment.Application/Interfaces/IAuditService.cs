namespace ConstructionPayment.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string entityName, string entityId, object? oldValue, object? newValue, CancellationToken cancellationToken = default);
}
