using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class WorkloadService : IWorkloadService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITaskStatusService _statusService;

    public WorkloadService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITaskStatusService statusService)
    {
        _context = context;
        _userManager = userManager;
        _statusService = statusService;
    }

    public async Task<List<UserWorkloadViewModel>> GetWorkloadsAsync()
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserWorkloadViewModel>();

        foreach (var user in users)
        {
            if (!await _userManager.IsInRoleAsync(user, "Executor") && !await _userManager.IsInRoleAsync(user, "Manager"))
            {
                continue;
            }

            var tasks = await _context.TaskItems
                .Where(t => t.AssignedToId == user.Id && t.Status != TaskItemStatus.Completed && t.Status != TaskItemStatus.Cancelled)
                .ToListAsync();

            var active = tasks.Count(t => _statusService.ResolveStatus(t) != TaskItemStatus.Overdue);
            var overdue = tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue);
            var critical = tasks.Count(t => t.Priority == TaskPriority.Critical);
            var review = tasks.Count(t => t.Status == TaskItemStatus.Review);
            var hours = tasks.Sum(t => Math.Max(t.EstimatedHours - (t.EstimatedHours * t.ProgressPercent / 100), 0));
            var score = active * 12 + overdue * 25 + critical * 15 + review * 8 + hours;
            var level = ResolveLevel(score);

            result.Add(new UserWorkloadViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Position = user.Position,
                ActiveTasks = active,
                ReviewTasks = review,
                OverdueTasks = overdue,
                CriticalTasks = critical,
                EstimatedHours = hours,
                WorkloadScore = score,
                Level = level.Label,
                LevelClass = level.CssClass
            });
        }

        return result.OrderBy(w => w.WorkloadScore).ToList();
    }

    public async Task<List<SuggestedAssigneeViewModel>> GetSuggestionsAsync(int? teamId = null)
    {
        var workloads = await GetWorkloadsAsync();

        if (teamId.HasValue)
        {
            var teamUserIds = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId.Value)
                .Select(tm => tm.UserId)
                .ToListAsync();

            workloads = workloads.Where(w => teamUserIds.Contains(w.UserId)).ToList();
        }

        return workloads
            .OrderBy(w => w.WorkloadScore)
            .Take(5)
            .Select(w => new SuggestedAssigneeViewModel
            {
                UserId = w.UserId,
                FullName = w.FullName,
                Position = w.Position,
                ActiveTasks = w.ActiveTasks,
                ReviewTasks = w.ReviewTasks,
                OverdueTasks = w.OverdueTasks,
                CriticalTasks = w.CriticalTasks,
                EstimatedHours = w.EstimatedHours,
                WorkloadScore = w.WorkloadScore,
                Level = w.Level,
                LevelClass = w.LevelClass,
                RecommendationReason = w.OverdueTasks == 0
                    ? "Немає прострочених завдань, навантаження контрольоване"
                    : "Є прострочені завдання, призначати тільки за потреби"
            })
            .ToList();
    }

    private static (string Label, string CssClass) ResolveLevel(int score)
    {
        if (score >= 150)
        {
            return ("Перевантажений", "level-danger");
        }

        if (score >= 90)
        {
            return ("Високе", "level-warning");
        }

        if (score >= 45)
        {
            return ("Оптимальне", "level-primary");
        }

        return ("Вільний ресурс", "level-success");
    }
}
