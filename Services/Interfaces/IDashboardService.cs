using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(string userId, bool canViewAll);
}
