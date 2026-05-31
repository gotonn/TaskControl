using Microsoft.EntityFrameworkCore;
using TaskControl.Data;
using TaskControl.Models;
using TaskControl.Models.Enums;
using TaskControl.Services.Interfaces;
using TaskControl.ViewModels;

namespace TaskControl.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(string userId, string title, string message, NotificationType type, int? taskItemId = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            TaskItemId = taskItemId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<NotificationIndexViewModel> GetForUserAsync(string userId)
    {
        var items = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(80)
            .Select(n => new NotificationItemViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                TaskItemId = n.TaskItemId,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new NotificationIndexViewModel
        {
            Notifications = items,
            UnreadCount = items.Count(n => !n.IsRead)
        };
    }

    public Task<int> GetUnreadCountAsync(string userId)
    {
        return _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int id, string userId)
    {
        var item = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (item == null)
        {
            return;
        }

        item.IsRead = true;
        await _context.SaveChangesAsync();
    }
}
