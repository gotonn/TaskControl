using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITaskStatusService _statusService;
    private readonly IWorkloadService _workloadService;

    public DashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITaskStatusService statusService, IWorkloadService workloadService)
    {
        _context = context;
        _userManager = userManager;
        _statusService = statusService;
        _workloadService = workloadService;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(string userId, bool canViewAll)
    {
        var query = _context.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Team)
            .Include(t => t.Comments)
            .AsQueryable();

        if (!canViewAll)
        {
            query = query.Where(t => t.AssignedToId == userId || t.CreatedById == userId);
        }

        var tasks = await query.ToListAsync();
        var cards = tasks.Select(t => new TaskCardViewModel
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description.Length > 160 ? t.Description[..160] + "..." : t.Description,
            Priority = t.Priority,
            Status = _statusService.ResolveStatus(t),
            Deadline = t.Deadline,
            EstimatedHours = t.EstimatedHours,
            ProgressPercent = t.ProgressPercent,
            AssignedTo = t.AssignedTo.FullName,
            AssignedToId = t.AssignedToId,
            ProjectName = t.Project.Name,
            TeamName = t.Team.Name,
            RiskLabel = _statusService.GetRiskLabel(t),
            RiskClass = _statusService.GetRiskClass(t),
            StatusLabel = _statusService.GetStatusLabel(_statusService.ResolveStatus(t)),
            StatusClass = _statusService.GetStatusClass(_statusService.ResolveStatus(t)),
            PriorityLabel = _statusService.GetPriorityLabel(t.Priority),
            PriorityClass = _statusService.GetPriorityClass(t.Priority),
            CommentsCount = t.Comments.Count
        }).ToList();

        var completed = cards.Count(t => t.Status == TaskItemStatus.Completed);
        var total = cards.Count;

        return new DashboardViewModel
        {
            TotalTasks = total,
            ActiveTasks = cards.Count(t => t.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled and not TaskItemStatus.Overdue),
            CompletedTasks = completed,
            OverdueTasks = cards.Count(t => t.Status == TaskItemStatus.Overdue),
            HighRiskTasks = tasks.Count(_statusService.IsHighRisk),
            CompletionRate = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1),
            ProjectsCount = canViewAll ? await _context.WorkProjects.CountAsync() : await _context.WorkProjects.CountAsync(p => p.Tasks.Any(t => t.AssignedToId == userId || t.CreatedById == userId)),
            TeamsCount = canViewAll ? await _context.Teams.CountAsync() : await _context.TeamMembers.CountAsync(tm => tm.UserId == userId),
            UsersCount = canViewAll ? await _userManager.Users.CountAsync() : 1,
            NearestDeadlines = cards
                .Where(t => t.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled and not TaskItemStatus.Overdue)
                .OrderBy(t => t.Deadline)
                .Take(6)
                .ToList(),
            RiskyTasks = cards.Where(t => t.RiskClass is "risk-critical" or "risk-high").OrderBy(t => t.Deadline).Take(6).ToList(),
            Workloads = canViewAll ? await _workloadService.GetWorkloadsAsync() : new List<UserWorkloadViewModel>(),
            ProjectProgress = await GetProjectProgressAsync(canViewAll, userId)
        };
    }

    private async Task<List<ProjectProgressViewModel>> GetProjectProgressAsync(bool canViewAll, string userId)
    {
        var projects = await _context.WorkProjects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .ToListAsync();

        if (!canViewAll)
        {
            projects = projects.Where(p => p.Tasks.Any(t => t.AssignedToId == userId || t.CreatedById == userId)).ToList();
        }

        return projects.Select(p =>
        {
            var total = p.Tasks.Count;
            var completed = p.Tasks.Count(t => t.Status == TaskItemStatus.Completed);
            var overdue = p.Tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue);

            return new ProjectProgressViewModel
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                TeamName = p.Team.Name,
                TotalTasks = total,
                CompletedTasks = completed,
                OverdueTasks = overdue,
                Progress = total == 0 ? 0 : completed * 100 / total
            };
        }).OrderByDescending(p => p.TotalTasks).Take(8).ToList();
    }
}
