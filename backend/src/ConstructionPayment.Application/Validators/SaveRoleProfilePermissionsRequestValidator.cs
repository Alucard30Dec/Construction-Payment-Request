using ConstructionPayment.Application.Dtos.RolePermissions;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class SaveRoleProfilePermissionsRequestValidator : AbstractValidator<SaveRoleProfilePermissionsRequest>
{
    public SaveRoleProfilePermissionsRequestValidator()
    {
        RuleForEach(x => x.GrantedPermissionCodes)
            .NotEmpty()
            .MaximumLength(120);
    }
}
