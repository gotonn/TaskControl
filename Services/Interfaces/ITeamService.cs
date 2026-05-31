using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface ITeamService
{
    Task<TeamIndexViewModel> GetIndexAsync();
    Task<TeamDetailsViewModel?> GetDetailsAsync(int id);
}
