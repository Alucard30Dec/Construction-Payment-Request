using ConstructionPayment.Application.Common;
using ConstructionPayment.Application.Dtos.Projects;

namespace ConstructionPayment.Application.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetPagedAsync(ProjectQueryRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
