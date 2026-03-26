using ConstructionPayment.Application.Dtos.RolePermissions;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class UpdateRoleProfileRequestValidator : AbstractValidator<UpdateRoleProfileRequest>
{
    public UpdateRoleProfileRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
