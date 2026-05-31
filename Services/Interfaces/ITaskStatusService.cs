using TaskControl.Models;
using TaskControl.Models.Enums;

namespace TaskControl.Services.Interfaces;

public interface ITaskStatusService
{
    TaskItemStatus ResolveStatus(TaskItem task);
    string GetStatusLabel(TaskItemStatus status);
    string GetStatusClass(TaskItemStatus status);
    string GetPriorityLabel(TaskPriority priority);
    string GetPriorityClass(TaskPriority priority);
    string GetRiskLabel(TaskItem task);
    string GetRiskClass(TaskItem task);
    bool IsHighRisk(TaskItem task);
}
