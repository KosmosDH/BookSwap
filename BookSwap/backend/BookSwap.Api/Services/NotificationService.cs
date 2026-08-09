using BookSwap.Api.Data;
using BookSwap.Api.Models;

namespace BookSwap.Api.Services;

public sealed class NotificationService(AppDbContext context)
{
    public async Task CreateAsync(Guid recipientId, NotificationType type, string title, string body, string? link = null)
    {
        context.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            Type = type,
            Title = title,
            Body = body,
            Link = link
        });
        await context.SaveChangesAsync();
    }
}
