using System.Security.Claims;
using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Hubs;

[Authorize]
public sealed class ChatHub(AppDbContext context) : Hub
{
    public async Task SendMessage(Guid receiverId, string content)
    {
        var senderIdText = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(senderIdText, out var senderId)) throw new HubException("Пользователь не авторизован.");
        content = content.Trim();
        if (content.Length is 0 or > 2000) throw new HubException("Сообщение должно содержать от 1 до 2000 символов.");
        if (senderId == receiverId) throw new HubException("Нельзя отправить сообщение самому себе.");

        var sender = await context.Users.FindAsync(senderId) ?? throw new HubException("Отправитель не найден.");
        var receiver = await context.Users.FindAsync(receiverId) ?? throw new HubException("Получатель не найден.");
        if (receiver.IsBlocked) throw new HubException("Получатель недоступен.");

        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content
        };
        context.ChatMessages.Add(message);
        context.Notifications.Add(new Notification
        {
            RecipientId = receiverId,
            Type = NotificationType.Message,
            Title = "Новое сообщение",
            Body = $"{sender.DisplayName}: {content[..Math.Min(80, content.Length)]}",
            Link = $"/chat/{senderId}"
        });
        await context.SaveChangesAsync();

        var dto = new ChatMessageDto(
            message.Id,
            await sender.ToSummaryAsync(context),
            await receiver.ToSummaryAsync(context),
            message.Content,
            message.SentAt,
            message.ReadAt);

        await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", dto);
        await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", dto);
    }
}
