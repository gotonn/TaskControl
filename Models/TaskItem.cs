using System.ComponentModel.DataAnnotations;
using TaskControl.Models.Enums;

namespace TaskControl.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskItemStatus Status { get; set; } = TaskItemStatus.New;

    public DateTime Deadline { get; set; } = DateTime.UtcNow.Date.AddDays(7);

    public int EstimatedHours { get; set; } = 4;

    public int ProgressPercent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser Creator { get; set; } = null!;

    public string AssignedToId { get; set; } = string.Empty;

    public ApplicationUser AssignedTo { get; set; } = null!;

    public int ProjectId { get; set; }

    public WorkProject Project { get; set; } = null!;

    public int TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();

    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
}
