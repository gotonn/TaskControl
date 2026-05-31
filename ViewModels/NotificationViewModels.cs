using TaskControl.Models.Enums;

namespace TaskControl.ViewModels;

public class NotificationItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public int? TaskItemId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationIndexViewModel
{
    public List<NotificationItemViewModel> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
}
