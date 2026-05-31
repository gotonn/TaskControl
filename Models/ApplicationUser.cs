using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskControl.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(80)]
    public string Position { get; set; } = string.Empty;

    [StringLength(80)]
    public string Department { get; set; } = string.Empty;

    [StringLength(240)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityAt { get; set; }

    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

    public ICollection<TaskHistory> HistoryRecords { get; set; } = new List<TaskHistory>();

    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
