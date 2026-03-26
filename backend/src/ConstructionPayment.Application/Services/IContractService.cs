using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Contracts;

namespace ConstructionPayment.Application.Services;

public interface IContractService
{
    Task<PagedResult<ContractDto>> GetPagedAsync(ContractQueryRequest request, CancellationToken cancellationToken = default);
    Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default);
    Task<ContractDto> UpdateAsync(Guid id, UpdateContractRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
