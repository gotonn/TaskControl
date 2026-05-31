using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models.Enums;

public enum WorkProjectStatus
{
    [Display(Name = "Заплановано")]
    Planned = 0,

    [Display(Name = "Активний")]
    Active = 1,

    [Display(Name = "Призупинено")]
    Paused = 2,

    [Display(Name = "Виконано")]
    Completed = 3,

    [Display(Name = "Архів")]
    Archived = 4
}
