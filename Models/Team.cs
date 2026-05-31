using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models;

public class Team
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(800)]
    public string Description { get; set; } = string.Empty;

    [StringLength(40)]
    public string Color { get; set; } = "#4f46e5";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ManagerId { get; set; }

    public ApplicationUser? Manager { get; set; }

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    public ICollection<WorkProject> Projects { get; set; } = new List<WorkProject>();

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
