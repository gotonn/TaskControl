using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskControl.Services.Interfaces;

namespace TaskControl.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var canViewAll = User.IsInRole("Admin") || User.IsInRole("Manager");
        var model = await _dashboardService.GetDashboardAsync(userId, canViewAll);
        return View(model);
    }
}
