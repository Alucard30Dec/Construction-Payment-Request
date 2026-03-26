using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.PaymentRequests;

namespace ConstructionPayment.Application.Services;

public interface IPaymentRequestService
{
    Task<PagedResult<PaymentRequestDto>> GetPagedAsync(PaymentRequestQueryRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> CreateAsync(CreatePaymentRequestRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> UpdateAsync(Guid id, UpdatePaymentRequestRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaymentRequestDetailDto> SubmitAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> ApproveAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> RejectAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> ReturnForEditAsync(Guid id, WorkflowActionRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRequestDetailDto> ConfirmPaymentAsync(Guid id, PaymentConfirmationRequest request, CancellationToken cancellationToken = default);
}
