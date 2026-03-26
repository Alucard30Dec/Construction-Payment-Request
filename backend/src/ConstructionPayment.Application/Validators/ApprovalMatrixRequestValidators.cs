using ConstructionPayment.Application.Dtos.ApprovalMatrices;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class CreateApprovalMatrixRequestValidator : AbstractValidator<CreateApprovalMatrixRequest>
{
    public CreateApprovalMatrixRequestValidator()
    {
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAmount).GreaterThan(x => x.MinAmount);
    }
}

public class UpdateApprovalMatrixRequestValidator : AbstractValidator<UpdateApprovalMatrixRequest>
{
    public UpdateApprovalMatrixRequestValidator()
    {
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAmount).GreaterThan(x => x.MinAmount);
    }
}
