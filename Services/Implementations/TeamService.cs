using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskStatusService _statusService;
    private readonly IWorkloadService _workloadService;

    public TeamService(ApplicationDbContext context, ITaskStatusService statusService, IWorkloadService workloadService)
    {
        _context = context;
        _statusService = statusService;
        _workloadService = workloadService;
    }

    public async Task<TeamIndexViewModel> GetIndexAsync()
    {
        var teams = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .Include(t => t.Tasks)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return new TeamIndexViewModel
        {
            Teams = teams.Select(t => new TeamCardViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ManagerName = t.Manager?.FullName ?? "Не призначено",
                MembersCount = t.Members.Count,
                ProjectsCount = t.Projects.Count,
                ActiveTasks = t.Tasks.Count(x => x.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled),
                OverdueTasks = t.Tasks.Count(x => _statusService.ResolveStatus(x) == TaskItemStatus.Overdue)
            }).ToList()
        };
    }

    public async Task<TeamDetailsViewModel?> GetDetailsAsync(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Projects).ThenInclude(p => p.Tasks)
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return null;
        }

        var workloads = await _workloadService.GetWorkloadsAsync();
        var memberIds = team.Members.Select(m => m.UserId).ToHashSet();

        return new TeamDetailsViewModel
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            ManagerName = team.Manager?.FullName ?? "Не призначено",
            MembersCount = team.Members.Count,
            ProjectsCount = team.Projects.Count,
            ActiveTasks = team.Tasks.Count(t => t.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled),
            OverdueTasks = team.Tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue),
            Members = workloads.Where(w => memberIds.Contains(w.UserId)).ToList(),
            Projects = team.Projects.Select(p =>
            {
                var total = p.Tasks.Count;
                var completed = p.Tasks.Count(t => t.Status == TaskItemStatus.Completed);
                return new ProjectProgressViewModel
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    TeamName = team.Name,
                    TotalTasks = total,
                    CompletedTasks = completed,
                    OverdueTasks = p.Tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue),
                    Progress = total == 0 ? 0 : completed * 100 / total
                };
            }).ToList()
        };
    }
}
