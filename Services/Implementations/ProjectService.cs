using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskStatusService _statusService;

    public ProjectService(ApplicationDbContext context, ITaskStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<ProjectIndexViewModel> GetIndexAsync()
    {
        var projects = await _context.WorkProjects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return new ProjectIndexViewModel
        {
            Projects = projects.Select(MapProgress).ToList()
        };
    }

    public async Task<ProjectDetailsViewModel?> GetDetailsAsync(int id)
    {
        var project = await _context.WorkProjects
            .Include(p => p.Team)
            .Include(p => p.Tasks).ThenInclude(t => t.AssignedTo)
            .Include(p => p.Tasks).ThenInclude(t => t.Team)
            .Include(p => p.Tasks).ThenInclude(t => t.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return null;
        }

        var progress = MapProgress(project);

        return new ProjectDetailsViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            TeamName = project.Team.Name,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Progress = progress.Progress,
            Tasks = project.Tasks.OrderBy(t => t.Deadline).Select(t => new TaskCardViewModel
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
                ProjectName = project.Name,
                TeamName = t.Team.Name,
                RiskLabel = _statusService.GetRiskLabel(t),
                RiskClass = _statusService.GetRiskClass(t),
                StatusLabel = _statusService.GetStatusLabel(_statusService.ResolveStatus(t)),
                StatusClass = _statusService.GetStatusClass(_statusService.ResolveStatus(t)),
                PriorityLabel = _statusService.GetPriorityLabel(t.Priority),
                PriorityClass = _statusService.GetPriorityClass(t.Priority),
                CommentsCount = t.Comments.Count
            }).ToList()
        };
    }

    public async Task<ProjectCreateViewModel> BuildCreateModelAsync()
    {
        return new ProjectCreateViewModel
        {
            Teams = await _context.Teams
                .OrderBy(t => t.Name)
                .Select(t => new SelectItemViewModel { Value = t.Id.ToString(), Text = t.Name })
                .ToListAsync()
        };
    }

    public async Task<int> CreateAsync(ProjectCreateViewModel model, string userId)
    {
        var project = new WorkProject
        {
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            StartDate = model.StartDate.ToUniversalTime(),
            EndDate = model.EndDate?.ToUniversalTime(),
            Status = model.Status,
            TeamId = model.TeamId,
            Color = model.Color,
            CreatedById = userId
        };

        _context.WorkProjects.Add(project);
        await _context.SaveChangesAsync();
        return project.Id;
    }

    private ProjectProgressViewModel MapProgress(WorkProject project)
    {
        var total = project.Tasks.Count;
        var completed = project.Tasks.Count(t => t.Status == TaskItemStatus.Completed);
        var overdue = project.Tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue);

        return new ProjectProgressViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            TeamName = project.Team.Name,
            TotalTasks = total,
            CompletedTasks = completed,
            OverdueTasks = overdue,
            Progress = total == 0 ? 0 : completed * 100 / total
        };
    }
}
