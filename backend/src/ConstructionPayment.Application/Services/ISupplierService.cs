using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Suppliers;

namespace ConstructionPayment.Application.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetPagedAsync(SupplierQueryRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
