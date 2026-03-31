namespace WizardRPG.Api.DTOs.Notification;

public record NotificationResponse(Guid Id, string Title, string Message, string Type, bool IsRead, DateTime CreatedAt);
