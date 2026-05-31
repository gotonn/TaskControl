using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;

namespace TaskControl.Services.Implementations;

public class TaskStatusService : ITaskStatusService
{
    public TaskItemStatus ResolveStatus(TaskItem task)
    {
        if (task.Status is TaskItemStatus.Completed or TaskItemStatus.Cancelled)
        {
            return task.Status;
        }

        return task.Deadline < DateTime.UtcNow ? TaskItemStatus.Overdue : task.Status;
    }

    public string GetStatusLabel(TaskItemStatus status) => status switch
    {
        TaskItemStatus.New => "Нове",
        TaskItemStatus.Assigned => "Призначено",
        TaskItemStatus.InProgress => "У процесі",
        TaskItemStatus.Review => "На перевірці",
        TaskItemStatus.Completed => "Виконано",
        TaskItemStatus.Overdue => "Прострочено",
        TaskItemStatus.Cancelled => "Скасовано",
        _ => "Невідомо"
    };

    public string GetStatusClass(TaskItemStatus status) => status switch
    {
        TaskItemStatus.New => "badge-soft-info",
        TaskItemStatus.Assigned => "badge-soft-primary",
        TaskItemStatus.InProgress => "badge-soft-warning",
        TaskItemStatus.Review => "badge-soft-purple",
        TaskItemStatus.Completed => "badge-soft-success",
        TaskItemStatus.Overdue => "badge-soft-danger",
        TaskItemStatus.Cancelled => "badge-soft-secondary",
        _ => "badge-soft-secondary"
    };

    public string GetPriorityLabel(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "Низький",
        TaskPriority.Medium => "Середній",
        TaskPriority.High => "Високий",
        TaskPriority.Critical => "Критичний",
        _ => "Середній"
    };

    public string GetPriorityClass(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "priority-low",
        TaskPriority.Medium => "priority-medium",
        TaskPriority.High => "priority-high",
        TaskPriority.Critical => "priority-critical",
        _ => "priority-medium"
    };

    public string GetRiskLabel(TaskItem task)
    {
        if (task.Status is TaskItemStatus.Completed or TaskItemStatus.Cancelled)
        {
            return "Без ризику";
        }

        var hoursLeft = (task.Deadline - DateTime.UtcNow).TotalHours;
        if (hoursLeft < 0)
        {
            return "Критичний ризик";
        }

        if (hoursLeft <= 24 && task.ProgressPercent < 80)
        {
            return "Високий ризик";
        }

        if (hoursLeft <= 72 && task.ProgressPercent < 50)
        {
            return "Середній ризик";
        }

        return "Низький ризик";
    }

    public string GetRiskClass(TaskItem task)
    {
        if (task.Status is TaskItemStatus.Completed or TaskItemStatus.Cancelled)
        {
            return "risk-none";
        }

        var hoursLeft = (task.Deadline - DateTime.UtcNow).TotalHours;
        if (hoursLeft < 0)
        {
            return "risk-critical";
        }

        if (hoursLeft <= 24 && task.ProgressPercent < 80)
        {
            return "risk-high";
        }

        if (hoursLeft <= 72 && task.ProgressPercent < 50)
        {
            return "risk-medium";
        }

        return "risk-low";
    }

    public bool IsHighRisk(TaskItem task)
    {
        var riskClass = GetRiskClass(task);
        return riskClass is "risk-critical" or "risk-high";
    }
}
