using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<IActionResult> Index([FromQuery] TaskFilterViewModel filter)
    {
        var model = await _taskService.GetListAsync(filter, CurrentUserId(), CanViewAll());
        return View(model);
    }

    public async Task<IActionResult> Kanban()
    {
        var model = await _taskService.GetKanbanAsync(CurrentUserId(), CanViewAll());
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _taskService.GetDetailsAsync(id, CurrentUserId(), CanViewAll());
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create()
    {
        return View(await _taskService.BuildCreateModelAsync());
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rebuilt = await _taskService.BuildCreateModelAsync();
            model.Users = rebuilt.Users;
            model.Projects = rebuilt.Projects;
            model.Suggestions = rebuilt.Suggestions;
            return View(model);
        }

        var id = await _taskService.CreateAsync(model, CurrentUserId());
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _taskService.BuildEditModelAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rebuilt = await _taskService.BuildCreateModelAsync();
            model.Users = rebuilt.Users;
            model.Projects = rebuilt.Projects;
            model.Suggestions = rebuilt.Suggestions;
            return View(model);
        }

        var updated = await _taskService.UpdateAsync(model, CurrentUserId());
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, TaskItemStatus status)
    {
        var updated = await _taskService.ChangeStatusAsync(id, status, CurrentUserId(), User.IsInRole("Admin") || User.IsInRole("Manager"));
        if (!updated)
        {
            return Forbid();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string text)
    {
        var added = await _taskService.AddCommentAsync(id, CurrentUserId(), text, CanViewAll());
        if (!added)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    private bool CanViewAll()
    {
        return User.IsInRole("Admin") || User.IsInRole("Manager");
    }
}
