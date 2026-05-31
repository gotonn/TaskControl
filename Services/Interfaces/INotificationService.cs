using TaskControl.Models.Enums;
using TaskControl.ViewModels;

namespace TaskControl.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string message, NotificationType type, int? taskItemId = null);
    Task<NotificationIndexViewModel> GetForUserAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int id, string userId);
}
