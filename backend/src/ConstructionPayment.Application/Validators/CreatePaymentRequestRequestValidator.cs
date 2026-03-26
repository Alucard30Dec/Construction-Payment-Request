using ConstructionPayment.Application.Dtos.PaymentRequests;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class CreatePaymentRequestRequestValidator : AbstractValidator<CreatePaymentRequestRequest>
{
    public CreatePaymentRequestRequestValidator()
    {
        RuleFor(x => x.RequestCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.RequestType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AmountBeforeVat).GreaterThan(0);
        RuleFor(x => x.VatRate).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.AdvanceDeduction).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RetentionAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherDeduction).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate);
    }
}
