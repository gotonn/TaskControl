using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskStatusService _statusService;
    private readonly IWorkloadService _workloadService;
    private readonly INotificationService _notificationService;

    public TaskService(ApplicationDbContext context, ITaskStatusService statusService, IWorkloadService workloadService, INotificationService notificationService)
    {
        _context = context;
        _statusService = statusService;
        _workloadService = workloadService;
        _notificationService = notificationService;
    }

    public async Task<TaskListViewModel> GetListAsync(TaskFilterViewModel filter, string userId, bool canViewAll)
    {
        var query = BaseTaskQuery();

        if (!canViewAll)
        {
            query = query.Where(t => t.AssignedToId == userId || t.CreatedById == userId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search) || t.Project.Name.Contains(search));
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AssignedToId))
        {
            query = query.Where(t => t.AssignedToId == filter.AssignedToId);
        }

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
        }

        var tasks = await query.ToListAsync();
        var mapped = tasks.Select(MapCard).ToList();

        if (filter.Status.HasValue)
        {
            mapped = mapped.Where(t => t.Status == filter.Status.Value).ToList();
        }

        mapped = filter.SortBy switch
        {
            "priority" => mapped.OrderByDescending(t => t.Priority).ThenBy(t => t.Deadline).ToList(),
            "status" => mapped.OrderBy(t => t.Status).ThenBy(t => t.Deadline).ToList(),
            "project" => mapped.OrderBy(t => t.ProjectName).ThenBy(t => t.Deadline).ToList(),
            _ => mapped.OrderBy(t => t.Deadline).ThenByDescending(t => t.Priority).ToList()
        };

        return new TaskListViewModel
        {
            Filter = filter,
            Tasks = mapped,
            TotalCount = mapped.Count,
            ActiveCount = mapped.Count(t => t.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled and not TaskItemStatus.Overdue),
            CompletedCount = mapped.Count(t => t.Status == TaskItemStatus.Completed),
            OverdueCount = mapped.Count(t => t.Status == TaskItemStatus.Overdue),
            Users = await GetUserSelectItemsAsync(),
            Projects = await GetProjectSelectItemsAsync()
        };
    }

    public async Task<KanbanViewModel> GetKanbanAsync(string userId, bool canViewAll)
    {
        var list = await GetListAsync(new TaskFilterViewModel(), userId, canViewAll);
        var statuses = new[]
        {
            TaskItemStatus.New,
            TaskItemStatus.Assigned,
            TaskItemStatus.InProgress,
            TaskItemStatus.Review,
            TaskItemStatus.Completed,
            TaskItemStatus.Overdue
        };

        return new KanbanViewModel
        {
            Columns = statuses.Select(status => new KanbanColumnViewModel
            {
                Status = status,
                Title = _statusService.GetStatusLabel(status),
                CssClass = _statusService.GetStatusClass(status),
                Tasks = list.Tasks.Where(t => t.Status == status).OrderBy(t => t.Deadline).ToList()
            }).ToList()
        };
    }

    public async Task<TaskDetailsViewModel?> GetDetailsAsync(int id, string userId, bool canViewAll)
    {
        var task = await BaseTaskQuery()
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .Include(t => t.History).ThenInclude(h => h.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return null;
        }

        if (!canViewAll && task.AssignedToId != userId && task.CreatedById != userId)
        {
            return null;
        }

        return new TaskDetailsViewModel
        {
            Task = MapCard(task),
            FullDescription = task.Description,
            CreatorName = task.Creator.FullName,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            Comments = task.Comments.OrderByDescending(c => c.CreatedAt).ToList(),
            History = task.History.OrderByDescending(h => h.CreatedAt).ToList(),
            Attachments = task.Attachments.OrderByDescending(a => a.UploadedAt).ToList()
        };
    }

    public async Task<TaskCreateViewModel> BuildCreateModelAsync()
    {
        return new TaskCreateViewModel
        {
            Users = await GetUserSelectItemsAsync(),
            Projects = await GetProjectSelectItemsAsync(),
            Suggestions = await _workloadService.GetSuggestionsAsync()
        };
    }

    public async Task<TaskEditViewModel?> BuildEditModelAsync(int id)
    {
        var task = await _context.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return null;
        }

        return new TaskEditViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            Deadline = task.Deadline.ToLocalTime(),
            EstimatedHours = task.EstimatedHours,
            AssignedToId = task.AssignedToId,
            ProjectId = task.ProjectId,
            ProgressPercent = task.ProgressPercent,
            Users = await GetUserSelectItemsAsync(),
            Projects = await GetProjectSelectItemsAsync(),
            Suggestions = await _workloadService.GetSuggestionsAsync()
        };
    }

    public async Task<int> CreateAsync(TaskCreateViewModel model, string creatorId)
    {
        var project = await _context.WorkProjects.AsNoTracking().FirstAsync(p => p.Id == model.ProjectId);
        var task = new TaskItem
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Priority = model.Priority,
            Status = model.Status,
            Deadline = model.Deadline.ToUniversalTime(),
            EstimatedHours = model.EstimatedHours,
            ProgressPercent = model.Status == TaskItemStatus.Completed ? 100 : 0,
            CreatedById = creatorId,
            AssignedToId = model.AssignedToId,
            ProjectId = model.ProjectId,
            TeamId = project.TeamId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (model.Status == TaskItemStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        await AddHistoryAsync(task.Id, creatorId, "Створено завдання", null, task.Title);
        await _notificationService.CreateAsync(task.AssignedToId, "Нове завдання", $"Вам призначено завдання «{task.Title}»", NotificationType.TaskAssigned, task.Id);
        return task.Id;
    }

    public async Task<bool> UpdateAsync(TaskEditViewModel model, string editorId)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == model.Id);
        if (task == null)
        {
            return false;
        }

        var oldStatus = task.Status;
        var oldAssignee = task.AssignedToId;
        var oldDeadline = task.Deadline;
        var project = await _context.WorkProjects.AsNoTracking().FirstAsync(p => p.Id == model.ProjectId);

        task.Title = model.Title.Trim();
        task.Description = model.Description.Trim();
        task.Priority = model.Priority;
        task.Status = model.Status;
        task.Deadline = model.Deadline.ToUniversalTime();
        task.EstimatedHours = model.EstimatedHours;
        task.AssignedToId = model.AssignedToId;
        task.ProjectId = model.ProjectId;
        task.TeamId = project.TeamId;
        task.ProgressPercent = Math.Clamp(model.ProgressPercent, 0, 100);
        task.UpdatedAt = DateTime.UtcNow;

        if (task.Status == TaskItemStatus.Completed && task.CompletedAt == null)
        {
            task.CompletedAt = DateTime.UtcNow;
            task.ProgressPercent = 100;
        }

        if (task.Status != TaskItemStatus.Completed)
        {
            task.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        if (oldStatus != task.Status)
        {
            await AddHistoryAsync(task.Id, editorId, "Змінено статус", _statusService.GetStatusLabel(oldStatus), _statusService.GetStatusLabel(task.Status));
        }

        if (oldAssignee != task.AssignedToId)
        {
            await AddHistoryAsync(task.Id, editorId, "Змінено виконавця", oldAssignee, task.AssignedToId);
            await _notificationService.CreateAsync(task.AssignedToId, "Завдання перепризначено", $"Вам призначено завдання «{task.Title}»", NotificationType.TaskAssigned, task.Id);
        }

        if (oldDeadline != task.Deadline)
        {
            await AddHistoryAsync(task.Id, editorId, "Змінено дедлайн", oldDeadline.ToLocalTime().ToString("dd.MM.yyyy HH:mm"), task.Deadline.ToLocalTime().ToString("dd.MM.yyyy HH:mm"));
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return false;
        }

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int id, TaskItemStatus status, string userId, bool canEditAny)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return false;
        }

        if (!canEditAny && task.AssignedToId != userId)
        {
            return false;
        }

        var old = task.Status;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        task.ProgressPercent = status switch
        {
            TaskItemStatus.New => 0,
            TaskItemStatus.Assigned => Math.Max(task.ProgressPercent, 0),
            TaskItemStatus.InProgress => Math.Max(task.ProgressPercent, 25),
            TaskItemStatus.Review => Math.Max(task.ProgressPercent, 80),
            TaskItemStatus.Completed => 100,
            _ => task.ProgressPercent
        };
        task.CompletedAt = status == TaskItemStatus.Completed ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
        await AddHistoryAsync(task.Id, userId, "Змінено статус", _statusService.GetStatusLabel(old), _statusService.GetStatusLabel(status));
        await _notificationService.CreateAsync(task.CreatedById, "Статус завдання змінено", $"Завдання «{task.Title}» отримало статус «{_statusService.GetStatusLabel(status)}»", NotificationType.StatusChanged, task.Id);
        return true;
    }

    public async Task<bool> AddCommentAsync(int id, string userId, string text, bool canViewAll)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return false;
        }

        if (!canViewAll && task.AssignedToId != userId && task.CreatedById != userId)
        {
            return false;
        }

        _context.TaskComments.Add(new TaskComment
        {
            TaskItemId = id,
            UserId = userId,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await AddHistoryAsync(id, userId, "Додано коментар", null, text.Trim());

        var targetUserId = task.AssignedToId == userId ? task.CreatedById : task.AssignedToId;
        await _notificationService.CreateAsync(targetUserId, "Новий коментар", $"До завдання «{task.Title}» додано коментар", NotificationType.CommentAdded, id);
        return true;
    }

    private IQueryable<TaskItem> BaseTaskQuery()
    {
        return _context.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.Creator)
            .Include(t => t.Project)
            .Include(t => t.Team)
            .Include(t => t.Comments);
    }

    private TaskCardViewModel MapCard(TaskItem task)
    {
        var status = _statusService.ResolveStatus(task);
        var description = task.Description.Length > 160 ? task.Description[..160] + "..." : task.Description;

        return new TaskCardViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = description,
            Priority = task.Priority,
            Status = status,
            Deadline = task.Deadline,
            EstimatedHours = task.EstimatedHours,
            ProgressPercent = task.ProgressPercent,
            AssignedTo = task.AssignedTo.FullName,
            AssignedToId = task.AssignedToId,
            ProjectName = task.Project.Name,
            TeamName = task.Team.Name,
            RiskLabel = _statusService.GetRiskLabel(task),
            RiskClass = _statusService.GetRiskClass(task),
            StatusLabel = _statusService.GetStatusLabel(status),
            StatusClass = _statusService.GetStatusClass(status),
            PriorityLabel = _statusService.GetPriorityLabel(task.Priority),
            PriorityClass = _statusService.GetPriorityClass(task.Priority),
            CommentsCount = task.Comments.Count
        };
    }

    private async Task<List<SelectItemViewModel>> GetUserSelectItemsAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new SelectItemViewModel { Value = u.Id, Text = u.FullName + " – " + u.Position })
            .ToListAsync();
    }

    private async Task<List<SelectItemViewModel>> GetProjectSelectItemsAsync()
    {
        return await _context.WorkProjects
            .Where(p => p.Status != WorkProjectStatus.Archived)
            .OrderBy(p => p.Name)
            .Select(p => new SelectItemViewModel { Value = p.Id.ToString(), Text = p.Name })
            .ToListAsync();
    }

    private async Task AddHistoryAsync(int taskId, string userId, string action, string? oldValue, string? newValue)
    {
        _context.TaskHistory.Add(new TaskHistory
        {
            TaskItemId = taskId,
            UserId = userId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
