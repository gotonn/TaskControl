using System.ComponentModel.DataAnnotations;
using TaskControl.Models.Enums;

namespace TaskControl.ViewModels;

public class ProjectIndexViewModel
{
    public List<ProjectProgressViewModel> Projects { get; set; } = new();
}

public class ProjectCreateViewModel
{
    [Required(ErrorMessage = "Введіть назву проєкту")]
    [StringLength(160, MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть опис проєкту")]
    [StringLength(1200, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Now.Date;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; } = DateTime.Now.Date.AddMonths(1);

    public WorkProjectStatus Status { get; set; } = WorkProjectStatus.Active;

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть команду")]
    public int TeamId { get; set; }

    public string Color { get; set; } = "#2563eb";

    public List<SelectItemViewModel> Teams { get; set; } = new();
}

public class ProjectDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public WorkProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Progress { get; set; }
    public List<TaskCardViewModel> Tasks { get; set; } = new();
}
