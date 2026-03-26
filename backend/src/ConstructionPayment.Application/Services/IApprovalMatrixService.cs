using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.ApprovalMatrices;

namespace ConstructionPayment.Application.Services;

public interface IApprovalMatrixService
{
    Task<PagedResult<ApprovalMatrixDto>> GetPagedAsync(ApprovalMatrixQueryRequest request, CancellationToken cancellationToken = default);
    Task<ApprovalMatrixDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApprovalMatrixDto> CreateAsync(CreateApprovalMatrixRequest request, CancellationToken cancellationToken = default);
    Task<ApprovalMatrixDto> UpdateAsync(Guid id, UpdateApprovalMatrixRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
