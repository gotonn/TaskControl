namespace TaskControl.ViewModels;

public class TeamIndexViewModel
{
    public List<TeamCardViewModel> Teams { get; set; } = new();
}

public class TeamCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int MembersCount { get; set; }
    public int ProjectsCount { get; set; }
    public int ActiveTasks { get; set; }
    public int OverdueTasks { get; set; }
}

public class TeamDetailsViewModel : TeamCardViewModel
{
    public List<UserWorkloadViewModel> Members { get; set; } = new();
    public List<ProjectProgressViewModel> Projects { get; set; } = new();
}
