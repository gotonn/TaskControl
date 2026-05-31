using TaskControl.Models.Enums;
using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface ITaskService
{
    Task<TaskListViewModel> GetListAsync(TaskFilterViewModel filter, string userId, bool canViewAll);
    Task<KanbanViewModel> GetKanbanAsync(string userId, bool canViewAll);
    Task<TaskDetailsViewModel?> GetDetailsAsync(int id, string userId, bool canViewAll);
    Task<TaskCreateViewModel> BuildCreateModelAsync();
    Task<TaskEditViewModel?> BuildEditModelAsync(int id);
    Task<int> CreateAsync(TaskCreateViewModel model, string creatorId);
    Task<bool> UpdateAsync(TaskEditViewModel model, string editorId);
    Task<bool> DeleteAsync(int id);
    Task<bool> ChangeStatusAsync(int id, TaskItemStatus status, string userId, bool canEditAny);
    Task<bool> AddCommentAsync(int id, string userId, string text, bool canViewAll);
}
