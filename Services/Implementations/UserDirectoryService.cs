using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class UserDirectoryService : IUserDirectoryService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITaskStatusService _statusService;

    public UserDirectoryService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITaskStatusService statusService)
    {
        _context = context;
        _userManager = userManager;
        _statusService = statusService;
    }

    public async Task<UserDirectoryViewModel> GetUsersAsync(string? search)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.FullName.Contains(term) || u.Email!.Contains(term) || u.Position.Contains(term) || u.Department.Contains(term));
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync();
        var result = new List<UserDirectoryItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tasks = await _context.TaskItems.Where(t => t.AssignedToId == user.Id).ToListAsync();

            result.Add(new UserDirectoryItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Position = user.Position,
                Department = user.Department,
                Roles = string.Join(", ", roles),
                IsActive = user.IsActive,
                ActiveTasks = tasks.Count(t => t.Status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled && _statusService.ResolveStatus(t) != TaskItemStatus.Overdue),
                CompletedTasks = tasks.Count(t => t.Status == TaskItemStatus.Completed),
                OverdueTasks = tasks.Count(t => _statusService.ResolveStatus(t) == TaskItemStatus.Overdue)
            });
        }

        return new UserDirectoryViewModel
        {
            Search = search,
            Users = result
        };
    }
}
