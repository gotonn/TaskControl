using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface IProjectService
{
    Task<ProjectIndexViewModel> GetIndexAsync();
    Task<ProjectDetailsViewModel?> GetDetailsAsync(int id);
    Task<ProjectCreateViewModel> BuildCreateModelAsync();
    Task<int> CreateAsync(ProjectCreateViewModel model, string userId);
}
