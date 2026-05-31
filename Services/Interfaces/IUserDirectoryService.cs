using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface IUserDirectoryService
{
    Task<UserDirectoryViewModel> GetUsersAsync(string? search);
}
