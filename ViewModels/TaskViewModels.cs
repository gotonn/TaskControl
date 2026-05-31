using System.ComponentModel.DataAnnotations;
using TaskControl.Models;
using TaskControl.Models.Enums;

namespace TaskControl.ViewModels;

public class TaskFilterViewModel
{
    public string? Search { get; set; }
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? AssignedToId { get; set; }
    public int? ProjectId { get; set; }
    public string SortBy { get; set; } = "deadline";
}

public class TaskListViewModel
{
    public TaskFilterViewModel Filter { get; set; } = new();
    public List<TaskCardViewModel> Tasks { get; set; } = new();
    public List<SelectItemViewModel> Users { get; set; } = new();
    public List<SelectItemViewModel> Projects { get; set; } = new();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
}

public class TaskCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime Deadline { get; set; }
    public int EstimatedHours { get; set; }
    public int ProgressPercent { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedToId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string RiskLabel { get; set; } = string.Empty;
    public string RiskClass { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public string PriorityLabel { get; set; } = string.Empty;
    public string PriorityClass { get; set; } = string.Empty;
    public int CommentsCount { get; set; }
}

public class TaskCreateViewModel
{
    [Required(ErrorMessage = "Введіть назву завдання")]
    [StringLength(180, MinimumLength = 4)]
    [Display(Name = "Назва")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть опис завдання")]
    [StringLength(3000, MinimumLength = 10)]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Пріоритет")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Display(Name = "Статус")]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Assigned;

    [Display(Name = "Дедлайн")]
    [DataType(DataType.DateTime)]
    public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

    [Range(1, 240, ErrorMessage = "Оцінка часу повинна бути від 1 до 240 годин")]
    [Display(Name = "Оцінка часу, год")]
    public int EstimatedHours { get; set; } = 4;

    [Required(ErrorMessage = "Оберіть виконавця")]
    [Display(Name = "Виконавець")]
    public string AssignedToId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть проєкт")]
    [Display(Name = "Проєкт")]
    public int ProjectId { get; set; }

    public List<SelectItemViewModel> Users { get; set; } = new();
    public List<SelectItemViewModel> Projects { get; set; } = new();
    public List<SuggestedAssigneeViewModel> Suggestions { get; set; } = new();
}

public class TaskEditViewModel : TaskCreateViewModel
{
    public int Id { get; set; }
    public int ProgressPercent { get; set; }
}

public class TaskDetailsViewModel
{
    public TaskCardViewModel Task { get; set; } = new();
    public string FullDescription { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<TaskComment> Comments { get; set; } = new();
    public List<TaskHistory> History { get; set; } = new();
    public List<TaskAttachment> Attachments { get; set; } = new();
    public string NewComment { get; set; } = string.Empty;
}

public class KanbanColumnViewModel
{
    public TaskItemStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public List<TaskCardViewModel> Tasks { get; set; } = new();
}

public class KanbanViewModel
{
    public List<KanbanColumnViewModel> Columns { get; set; } = new();
}
