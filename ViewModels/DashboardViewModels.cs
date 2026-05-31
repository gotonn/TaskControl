namespace TaskControl.ViewModels;

public class DashboardViewModel
{
    public int TotalTasks { get; set; }
    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int HighRiskTasks { get; set; }
    public int ProjectsCount { get; set; }
    public int TeamsCount { get; set; }
    public int UsersCount { get; set; }
    public double CompletionRate { get; set; }
    public List<TaskCardViewModel> NearestDeadlines { get; set; } = new();
    public List<TaskCardViewModel> RiskyTasks { get; set; } = new();
    public List<UserWorkloadViewModel> Workloads { get; set; } = new();
    public List<ProjectProgressViewModel> ProjectProgress { get; set; } = new();
}

public class UserWorkloadViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int ActiveTasks { get; set; }
    public int ReviewTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CriticalTasks { get; set; }
    public int EstimatedHours { get; set; }
    public int WorkloadScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public string LevelClass { get; set; } = string.Empty;
}

public class SuggestedAssigneeViewModel : UserWorkloadViewModel
{
    public string RecommendationReason { get; set; } = string.Empty;
}

public class ProjectProgressViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int Progress { get; set; }
}
