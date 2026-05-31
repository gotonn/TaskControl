using System.ComponentModel.DataAnnotations;

namespace TaskControl.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну пошту")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запам'ятати мене")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ПІБ")]
    [StringLength(120, MinimumLength = 4, ErrorMessage = "ПІБ повинен містити від 4 до 120 символів")]
    [Display(Name = "ПІБ")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну пошту")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть посаду")]
    [StringLength(80)]
    [Display(Name = "Посада")]
    public string Position { get; set; } = "Виконавець";

    [Required(ErrorMessage = "Введіть відділ")]
    [StringLength(80)]
    [Display(Name = "Відділ")]
    public string Department { get; set; } = "Загальний відділ";

    [Required(ErrorMessage = "Введіть пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль повинен містити щонайменше 6 символів")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не збігаються")]
    [Display(Name = "Підтвердження пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
