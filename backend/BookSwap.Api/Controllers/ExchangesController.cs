using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/exchanges")]
public sealed class ExchangesController(AppDbContext context, CurrentUserService currentUser, NotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExchangeDto>>> GetMine(string scope = "all")
    {
        var query = BaseQuery().Where(item => item.SenderId == currentUser.UserId || item.ReceiverId == currentUser.UserId);
        if (scope == "incoming") query = query.Where(item => item.ReceiverId == currentUser.UserId);
        if (scope == "outgoing") query = query.Where(item => item.SenderId == currentUser.UserId);
        var exchanges = await query.OrderByDescending(item => item.CreatedAt).ToListAsync();
        var result = new List<ExchangeDto>();
        foreach (var exchange in exchanges) result.Add(await ToDtoAsync(exchange));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeDto>> Create(CreateExchangeRequest request)
    {
        var requested = await context.Books.Include(book => book.Owner).FirstOrDefaultAsync(book => book.Id == request.RequestedBookId);
        if (requested is null || requested.Status != BookStatus.Available) return BadRequest(new { message = "Книга недоступна для обмена." });
        if (requested.OwnerId == currentUser.UserId) return BadRequest(new { message = "Нельзя отправить заявку на собственную книгу." });

        Book? offered = null;
        if (request.OfferedBookId.HasValue)
        {
            offered = await context.Books.FirstOrDefaultAsync(book => book.Id == request.OfferedBookId && book.OwnerId == currentUser.UserId);
            if (offered is null || offered.Status != BookStatus.Available)
                return BadRequest(new { message = "Предлагаемая книга недоступна или не принадлежит вам." });
        }
        if (requested.DealType == DealType.Exchange && offered is null)
            return BadRequest(new { message = "Для обмена необходимо выбрать предлагаемую книгу." });

        var duplicate = await context.ExchangeRequests.AnyAsync(item =>
            item.SenderId == currentUser.UserId && item.RequestedBookId == requested.Id && item.Status == ExchangeStatus.Pending);
        if (duplicate) return Conflict(new { message = "Активная заявка на эту книгу уже существует." });

        var exchange = new ExchangeRequest
        {
            SenderId = currentUser.UserId,
            ReceiverId = requested.OwnerId,
            RequestedBookId = requested.Id,
            OfferedBookId = offered?.Id,
            Message = request.Message?.Trim()
        };
        context.ExchangeRequests.Add(exchange);
        await context.SaveChangesAsync();
        await notifications.CreateAsync(requested.OwnerId, NotificationType.Exchange, "Новая заявка", $"На книгу «{requested.Title}» поступила заявка.", "/dashboard?tab=exchanges");

        var loaded = await BaseQuery().FirstAsync(item => item.Id == exchange.Id);
        return CreatedAtAction(nameof(GetMine), await ToDtoAsync(loaded));
    }

    [HttpPatch("{id:guid}/accept")]
    public async Task<ActionResult<ExchangeDto>> Accept(Guid id)
    {
        var exchange = await context.ExchangeRequests.Include(item => item.RequestedBook).Include(item => item.OfferedBook).FirstOrDefaultAsync(item => item.Id == id);
        if (exchange is null) return NotFound();
        if (exchange.ReceiverId != currentUser.UserId) return Forbid();
        if (exchange.Status != ExchangeStatus.Pending) return BadRequest(new { message = "Заявка уже обработана." });
        if (exchange.RequestedBook.Status != BookStatus.Available || exchange.OfferedBook is { Status: not BookStatus.Available })
            return BadRequest(new { message = "Одна из книг уже недоступна." });

        await using var transaction = await context.Database.BeginTransactionAsync();
        exchange.Status = ExchangeStatus.Accepted;
        exchange.UpdatedAt = DateTime.UtcNow;
        exchange.RequestedBook.Status = BookStatus.Reserved;
        if (exchange.OfferedBook is not null) exchange.OfferedBook.Status = BookStatus.Reserved;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        await notifications.CreateAsync(exchange.SenderId, NotificationType.Exchange, "Заявка принята", $"Ваша заявка на «{exchange.RequestedBook.Title}» принята.", "/dashboard?tab=exchanges");
        return Ok(await ReloadDtoAsync(id));
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<ActionResult<ExchangeDto>> Reject(Guid id)
    {
        var exchange = await context.ExchangeRequests.Include(item => item.RequestedBook).FirstOrDefaultAsync(item => item.Id == id);
        if (exchange is null) return NotFound();
        if (exchange.ReceiverId != currentUser.UserId) return Forbid();
        if (exchange.Status != ExchangeStatus.Pending) return BadRequest(new { message = "Заявка уже обработана." });
        exchange.Status = ExchangeStatus.Rejected;
        exchange.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        await notifications.CreateAsync(exchange.SenderId, NotificationType.Exchange, "Заявка отклонена", $"Заявка на «{exchange.RequestedBook.Title}» отклонена.", "/dashboard?tab=exchanges");
        return Ok(await ReloadDtoAsync(id));
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<ExchangeDto>> Cancel(Guid id)
    {
        var exchange = await context.ExchangeRequests.Include(item => item.RequestedBook).Include(item => item.OfferedBook).FirstOrDefaultAsync(item => item.Id == id);
        if (exchange is null) return NotFound();
        if (exchange.SenderId != currentUser.UserId) return Forbid();
        if (exchange.Status is not (ExchangeStatus.Pending or ExchangeStatus.Accepted)) return BadRequest(new { message = "Заявку уже нельзя отменить." });
        if (exchange.Status == ExchangeStatus.Accepted)
        {
            exchange.RequestedBook.Status = BookStatus.Available;
            if (exchange.OfferedBook is not null) exchange.OfferedBook.Status = BookStatus.Available;
        }
        exchange.Status = ExchangeStatus.Cancelled;
        exchange.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        await notifications.CreateAsync(exchange.ReceiverId, NotificationType.Exchange, "Заявка отменена", $"Заявка на «{exchange.RequestedBook.Title}» отменена.", "/dashboard?tab=exchanges");
        return Ok(await ReloadDtoAsync(id));
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<ActionResult<ExchangeDto>> Complete(Guid id)
    {
        var exchange = await context.ExchangeRequests.Include(item => item.RequestedBook).Include(item => item.OfferedBook).FirstOrDefaultAsync(item => item.Id == id);
        if (exchange is null) return NotFound();
        if (exchange.SenderId != currentUser.UserId && exchange.ReceiverId != currentUser.UserId) return Forbid();
        if (exchange.Status != ExchangeStatus.Accepted) return BadRequest(new { message = "Завершить можно только принятую заявку." });

        await using var transaction = await context.Database.BeginTransactionAsync();
        exchange.Status = ExchangeStatus.Completed;
        exchange.UpdatedAt = DateTime.UtcNow;
        exchange.RequestedBook.Status = BookStatus.Exchanged;
        if (exchange.OfferedBook is not null) exchange.OfferedBook.Status = BookStatus.Exchanged;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        var other = exchange.SenderId == currentUser.UserId ? exchange.ReceiverId : exchange.SenderId;
        await notifications.CreateAsync(other, NotificationType.Exchange, "Обмен завершён", $"Обмен книги «{exchange.RequestedBook.Title}» отмечен завершённым.", "/dashboard?tab=exchanges");
        return Ok(await ReloadDtoAsync(id));
    }

    private IQueryable<ExchangeRequest> BaseQuery() => context.ExchangeRequests
        .AsNoTracking()
        .Include(item => item.Sender)
        .Include(item => item.Receiver)
        .Include(item => item.RequestedBook).ThenInclude(book => book.Owner)
        .Include(item => item.RequestedBook).ThenInclude(book => book.Author)
        .Include(item => item.RequestedBook).ThenInclude(book => book.Photos)
        .Include(item => item.RequestedBook).ThenInclude(book => book.BookGenres).ThenInclude(item => item.Genre)
        .Include(item => item.OfferedBook).ThenInclude(book => book!.Owner)
        .Include(item => item.OfferedBook).ThenInclude(book => book!.Author)
        .Include(item => item.OfferedBook).ThenInclude(book => book!.Photos)
        .Include(item => item.OfferedBook).ThenInclude(book => book!.BookGenres).ThenInclude(item => item.Genre);

    private async Task<ExchangeDto> ReloadDtoAsync(Guid id) => await ToDtoAsync(await BaseQuery().FirstAsync(item => item.Id == id));

    private async Task<ExchangeDto> ToDtoAsync(ExchangeRequest exchange)
    {
        var sender = await exchange.Sender.ToSummaryAsync(context);
        var receiver = await exchange.Receiver.ToSummaryAsync(context);
        var requested = await exchange.RequestedBook.ToListItemAsync(context, currentUser.UserId);
        var offered = exchange.OfferedBook is null ? null : await exchange.OfferedBook.ToListItemAsync(context, currentUser.UserId);
        return new ExchangeDto(
            exchange.Id, sender, receiver, requested, offered, exchange.Message, exchange.Status, exchange.CreatedAt, exchange.UpdatedAt,
            exchange.ReceiverId == currentUser.UserId && exchange.Status == ExchangeStatus.Pending,
            exchange.SenderId == currentUser.UserId && exchange.Status is ExchangeStatus.Pending or ExchangeStatus.Accepted,
            (exchange.SenderId == currentUser.UserId || exchange.ReceiverId == currentUser.UserId) && exchange.Status == ExchangeStatus.Accepted);
    }
}
