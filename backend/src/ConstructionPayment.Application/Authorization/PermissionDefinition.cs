namespace ConstructionPayment.Application.Authorization;

public record PermissionDefinition(
    string Code,
    string Name,
    string Group,
    string Description);
