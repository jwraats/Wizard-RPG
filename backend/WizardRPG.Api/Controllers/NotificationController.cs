using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Notification;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/notification")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService) => _notificationService = notificationService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's notifications.</summary>
    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetMyNotifications([FromQuery] int limit = 20)
    {
        var notifications = await _notificationService.GetPlayerNotificationsAsync(CurrentPlayerId, limit);
        return Ok(notifications);
    }

    /// <summary>Get unread notification count.</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(CurrentPlayerId);
        return Ok(count);
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(CurrentPlayerId, id);
        return Ok(new { message = "Notification marked as read." });
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentPlayerId);
        return Ok(new { message = "All notifications marked as read." });
    }
}
