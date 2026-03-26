using ConstructionPayment.Application.Dtos.PaymentRequests;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class PaymentConfirmationRequestValidator : AbstractValidator<PaymentConfirmationRequest>
{
    public PaymentConfirmationRequestValidator()
    {
        RuleFor(x => x.PaymentDate).NotEmpty();
        RuleFor(x => x.PaidAmount).GreaterThan(0);
    }
}
