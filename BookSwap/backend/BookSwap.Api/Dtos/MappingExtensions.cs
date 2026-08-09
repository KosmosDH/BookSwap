using BookSwap.Api.Data;
using BookSwap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Dtos;

public static class MappingExtensions
{
    public static async Task<UserSummaryDto> ToSummaryAsync(this AppUser user, AppDbContext context)
    {
        var stats = await context.Reviews
            .Where(review => review.TargetUserId == user.Id)
            .GroupBy(_ => 1)
            .Select(group => new { Rating = group.Average(item => item.Rating), Count = group.Count() })
            .FirstOrDefaultAsync();
        return new UserSummaryDto(user.Id, user.DisplayName, user.City, user.AvatarUrl, stats?.Rating ?? 0, stats?.Count ?? 0);
    }

    public static async Task<BookListItemDto> ToListItemAsync(this Book book, AppDbContext context, Guid currentUserId = default)
    {
        var favorite = currentUserId != Guid.Empty && await context.Favorites.AnyAsync(item => item.UserId == currentUserId && item.BookId == book.Id);
        var owner = await book.Owner.ToSummaryAsync(context);
        return new BookListItemDto(
            book.Id,
            book.Title,
            book.Author.Name,
            book.City,
            book.Condition,
            book.DealType,
            book.Status,
            book.Price,
            book.Photos.OrderByDescending(photo => photo.IsPrimary).Select(photo => photo.Url).FirstOrDefault(),
            book.CreatedAt,
            owner,
            book.BookGenres.Select(item => item.Genre.Name).ToArray(),
            favorite);
    }
}
