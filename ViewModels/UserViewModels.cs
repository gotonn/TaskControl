namespace TaskControl.ViewModels;

public class UserDirectoryViewModel
{
    public string? Search { get; set; }
    public List<UserDirectoryItemViewModel> Users { get; set; } = new();
}

public class UserDirectoryItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}
