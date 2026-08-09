using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public sealed class MessagesController(AppDbContext context, CurrentUserService currentUser) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> Conversations()
    {
        var messages = await context.ChatMessages.AsNoTracking()
            .Include(item => item.Sender)
            .Include(item => item.Receiver)
            .Where(item => item.SenderId == currentUser.UserId || item.ReceiverId == currentUser.UserId)
            .OrderByDescending(item => item.SentAt)
            .ToListAsync();

        var groups = messages.GroupBy(item => item.SenderId == currentUser.UserId ? item.ReceiverId : item.SenderId);
        var result = new List<ConversationDto>();
        foreach (var group in groups)
        {
            var last = group.First();
            var partner = last.SenderId == currentUser.UserId ? last.Receiver : last.Sender;
            result.Add(new ConversationDto(
                await partner.ToSummaryAsync(context),
                last.Content,
                last.SentAt,
                group.Count(item => item.ReceiverId == currentUser.UserId && item.ReadAt == null)));
        }
        return Ok(result.OrderByDescending(item => item.LastMessageAt));
    }

    [HttpGet("{partnerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> History(Guid partnerId, int take = 100)
    {
        take = Math.Clamp(take, 1, 200);
        var messages = await context.ChatMessages
            .Include(item => item.Sender)
            .Include(item => item.Receiver)
            .Where(item =>
                (item.SenderId == currentUser.UserId && item.ReceiverId == partnerId) ||
                (item.SenderId == partnerId && item.ReceiverId == currentUser.UserId))
            .OrderByDescending(item => item.SentAt)
            .Take(take)
            .OrderBy(item => item.SentAt)
            .ToListAsync();

        var unread = messages.Where(item => item.ReceiverId == currentUser.UserId && item.ReadAt == null).ToList();
        foreach (var message in unread) message.ReadAt = DateTime.UtcNow;
        if (unread.Count > 0) await context.SaveChangesAsync();

        var result = new List<ChatMessageDto>();
        foreach (var message in messages)
        {
            result.Add(new ChatMessageDto(
                message.Id,
                await message.Sender.ToSummaryAsync(context),
                await message.Receiver.ToSummaryAsync(context),
                message.Content,
                message.SentAt,
                message.ReadAt));
        }
        return Ok(result);
    }
}
