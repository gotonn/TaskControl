using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface IWorkloadService
{
    Task<List<UserWorkloadViewModel>> GetWorkloadsAsync();
    Task<List<SuggestedAssigneeViewModel>> GetSuggestionsAsync(int? teamId = null);
}
