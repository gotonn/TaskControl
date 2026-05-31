using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models.Enums;

public enum TaskItemStatus
{
    [Display(Name = "Нове")]
    New = 0,

    [Display(Name = "Призначено")]
    Assigned = 1,

    [Display(Name = "У процесі")]
    InProgress = 2,

    [Display(Name = "На перевірці")]
    Review = 3,

    [Display(Name = "Виконано")]
    Completed = 4,

    [Display(Name = "Прострочено")]
    Overdue = 5,

    [Display(Name = "Скасовано")]
    Cancelled = 6
}
