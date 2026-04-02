using ConstructionPayment.Application.Dtos.PaymentRequests;
using FluentValidation;

namespace ConstructionPayment.Application.Validators;

public class UpdatePaymentRequestRequestValidator : AbstractValidator<UpdatePaymentRequestRequest>
{
    public UpdatePaymentRequestRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Vui lòng nhập tiêu đề.")
            .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự.");
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Vui lòng chọn dự án.");
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Vui lòng chọn nhà cung cấp.");
        RuleFor(x => x.RequestType)
            .NotEmpty().WithMessage("Vui lòng nhập loại đề nghị.")
            .MaximumLength(100).WithMessage("Loại đề nghị không được vượt quá 100 ký tự.");
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Vui lòng nhập số hóa đơn.")
            .MaximumLength(100).WithMessage("Số hóa đơn không được vượt quá 100 ký tự.");
        RuleFor(x => x.AmountBeforeVat)
            .GreaterThan(0).WithMessage("Số tiền trước VAT phải lớn hơn 0.");
        RuleFor(x => x.VatRate)
            .GreaterThanOrEqualTo(0).WithMessage("VAT không được âm.")
            .LessThanOrEqualTo(100).WithMessage("VAT không được vượt quá 100%.");
        RuleFor(x => x.AdvanceDeduction)
            .GreaterThanOrEqualTo(0).WithMessage("Khấu trừ tạm ứng không được âm.");
        RuleFor(x => x.RetentionAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Giữ lại bảo hành không được âm.");
        RuleFor(x => x.OtherDeduction)
            .GreaterThanOrEqualTo(0).WithMessage("Khấu trừ khác không được âm.");
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.InvoiceDate).WithMessage("Hạn thanh toán không được trước ngày hóa đơn.");
    }
}
