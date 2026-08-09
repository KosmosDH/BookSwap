using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController(AppDbContext context, CurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(int take = 30) => Ok(await context.Notifications
        .AsNoTracking()
        .Where(item => item.RecipientId == currentUser.UserId)
        .OrderByDescending(item => item.CreatedAt)
        .Take(Math.Clamp(take, 1, 100))
        .Select(item => new NotificationDto(item.Id, item.Type, item.Title, item.Body, item.Link, item.IsRead, item.CreatedAt))
        .ToListAsync());

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> UnreadCount() => Ok(new
    {
        count = await context.Notifications.CountAsync(item => item.RecipientId == currentUser.UserId && !item.IsRead)
    });

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id)
    {
        var item = await context.Notifications.FirstOrDefaultAsync(item => item.Id == id && item.RecipientId == currentUser.UserId);
        if (item is null) return NotFound();
        item.IsRead = true;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        await context.Notifications
            .Where(item => item.RecipientId == currentUser.UserId && !item.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsRead, true));
        return NoContent();
    }
}
