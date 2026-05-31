using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models.Enums;

public enum TaskPriority
{
    [Display(Name = "Низький")]
    Low = 0,

    [Display(Name = "Середній")]
    Medium = 1,

    [Display(Name = "Високий")]
    High = 2,

    [Display(Name = "Критичний")]
    Critical = 3
}
