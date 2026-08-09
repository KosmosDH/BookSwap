using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(AppDbContext context, CurrentUserService currentUser, NotificationService notifications) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> ForUser(Guid userId)
    {
        var reviews = await context.Reviews.AsNoTracking().Include(item => item.Author)
            .Where(item => item.TargetUserId == userId).OrderByDescending(item => item.CreatedAt).ToListAsync();
        var result = new List<ReviewDto>();
        foreach (var review in reviews)
            result.Add(new ReviewDto(review.Id, await review.Author.ToSummaryAsync(context), review.Rating, review.Text, review.CreatedAt));
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest request)
    {
        var exchange = await context.ExchangeRequests.FirstOrDefaultAsync(item => item.Id == request.ExchangeRequestId);
        if (exchange is null || exchange.Status != ExchangeStatus.Completed) return BadRequest(new { message = "Отзыв доступен только после завершённого обмена." });
        if (exchange.SenderId != currentUser.UserId && exchange.ReceiverId != currentUser.UserId) return Forbid();
        if (await context.Reviews.AnyAsync(item => item.AuthorId == currentUser.UserId && item.ExchangeRequestId == exchange.Id))
            return Conflict(new { message = "Вы уже оставили отзыв об этом обмене." });

        var targetId = exchange.SenderId == currentUser.UserId ? exchange.ReceiverId : exchange.SenderId;
        var review = new Review
        {
            AuthorId = currentUser.UserId,
            TargetUserId = targetId,
            ExchangeRequestId = exchange.Id,
            Rating = request.Rating,
            Text = request.Text.Trim()
        };
        context.Reviews.Add(review);
        await context.SaveChangesAsync();
        await notifications.CreateAsync(targetId, NotificationType.Review, "Новый отзыв", "После обмена вам оставили новый отзыв.", $"/users/{targetId}");
        var author = await context.Users.FindAsync(currentUser.UserId) ?? throw new InvalidOperationException();
        return Ok(new ReviewDto(review.Id, await author.ToSummaryAsync(context), review.Rating, review.Text, review.CreatedAt));
    }
}
