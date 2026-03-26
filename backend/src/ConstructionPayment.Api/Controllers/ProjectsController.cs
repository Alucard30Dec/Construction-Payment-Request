using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Dtos.Projects;
using ConstructionPayment.Application.Services;
using ConstructionPayment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionPayment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [PermissionAuthorize(PermissionCodes.ProjectsView)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ProjectQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [PermissionAuthorize(PermissionCodes.ProjectsView)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [PermissionAuthorize(PermissionCodes.ProjectsCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [PermissionAuthorize(PermissionCodes.ProjectsUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projectService.UpdateAsync(id, request, cancellationToken);
        return Ok(project);
    }

    [PermissionAuthorize(PermissionCodes.ProjectsDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
