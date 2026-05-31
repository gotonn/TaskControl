using System.ComponentModel.DataAnnotations;
using TaskControl.Models.Enums;

namespace TaskControl.Models;

public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(800)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public bool IsRead { get; set; }

    public int? TaskItemId { get; set; }

    public TaskItem? TaskItem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
