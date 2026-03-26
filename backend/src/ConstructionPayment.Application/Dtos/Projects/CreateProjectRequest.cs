namespace ConstructionPayment.Application.Dtos.Projects;

public class CreateProjectRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? ProjectManager { get; set; }
    public bool IsActive { get; set; } = true;
}
