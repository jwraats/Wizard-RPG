using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Notification;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetPlayerNotificationsAsync(Guid playerId, int limit = 20);
    Task<int> GetUnreadCountAsync(Guid playerId);
    Task MarkAsReadAsync(Guid playerId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid playerId);
    Task CreateNotificationAsync(Guid playerId, string title, string message, string type);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task<List<NotificationResponse>> GetPlayerNotificationsAsync(Guid playerId, int limit = 20)
    {
        var notifications = await _db.Notifications
            .Where(n => n.PlayerId == playerId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return notifications.Select(n => new NotificationResponse(
            n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt)).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid playerId)
    {
        return await _db.Notifications
            .CountAsync(n => n.PlayerId == playerId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(Guid playerId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid playerId)
    {
        var unread = await _db.Notifications
            .Where(n => n.PlayerId == playerId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
            n.IsRead = true;

        await _db.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(Guid playerId, string title, string message, string type)
    {
        var notification = new Notification
        {
            PlayerId = playerId,
            Title = title,
            Message = message,
            Type = type
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
}
