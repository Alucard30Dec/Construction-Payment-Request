using ConstructionPayment.Application.Dtos.PaymentRequests;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class PaymentConfirmationRequestValidator : AbstractValidator<PaymentConfirmationRequest>
{
    public PaymentConfirmationRequestValidator()
    {
        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Vui lòng chọn ngày thanh toán.");
        RuleFor(x => x.PaidAmount)
            .GreaterThan(0).WithMessage("Số tiền thanh toán phải lớn hơn 0.");
    }
}
