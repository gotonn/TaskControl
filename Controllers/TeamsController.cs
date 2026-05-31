using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskControl.Services.Interfaces;

namespace TaskControl.Controllers;

[Authorize]
public class TeamsController : Controller
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _teamService.GetIndexAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _teamService.GetDetailsAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }
}
