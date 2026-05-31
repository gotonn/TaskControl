using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskControl.Services.Interfaces;

namespace TaskControl.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class UsersController : Controller
{
    private readonly IUserDirectoryService _userDirectoryService;

    public UsersController(IUserDirectoryService userDirectoryService)
    {
        _userDirectoryService = userDirectoryService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        return View(await _userDirectoryService.GetUsersAsync(search));
    }
}
