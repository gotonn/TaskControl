using System.ComponentModel.DataAnnotations;
using TaskControl.Models.Enums;

namespace TaskControl.Models;

public class WorkProject
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    public DateTime? EndDate { get; set; }

    public WorkProjectStatus Status { get; set; } = WorkProjectStatus.Active;

    [StringLength(40)]
    public string Color { get; set; } = "#2563eb";

    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser CreatedBy { get; set; } = null!;

    public int TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
