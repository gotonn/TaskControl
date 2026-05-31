using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models;

public class TaskHistory
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(120)]
    public string Action { get; set; } = string.Empty;

    [StringLength(600)]
    public string? OldValue { get; set; }

    [StringLength(600)]
    public string? NewValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
