using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _projectService.GetIndexAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _projectService.GetDetailsAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create()
    {
        return View(await _projectService.BuildCreateModelAsync());
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rebuilt = await _projectService.BuildCreateModelAsync();
            model.Teams = rebuilt.Teams;
            return View(model);
        }

        var id = await _projectService.CreateAsync(model, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        return RedirectToAction(nameof(Details), new { id });
    }
}
