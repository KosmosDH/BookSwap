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
[Route("api/favorites")]
public sealed class FavoritesController(AppDbContext context, CurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> GetAll()
    {
        // Load favorites as the query root. Applying Include after projecting Favorite to Book
        // can fail in EF Core because Include must target an entity in the original query.
        var favorites = await context.Favorites
            .Where(item => item.UserId == currentUser.UserId)
            .OrderByDescending(item => item.CreatedAt)
            .Include(item => item.Book)
                .ThenInclude(book => book.Owner)
            .Include(item => item.Book)
                .ThenInclude(book => book.Author)
            .Include(item => item.Book)
                .ThenInclude(book => book.Photos)
            .Include(item => item.Book)
                .ThenInclude(book => book.BookGenres)
                    .ThenInclude(item => item.Genre)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<BookListItemDto>(favorites.Count);
        foreach (var favorite in favorites)
        {
            result.Add(await favorite.Book.ToListItemAsync(context, currentUser.UserId));
        }

        return Ok(result);
    }

    [HttpPost("{bookId:guid}")]
    public async Task<IActionResult> Add(Guid bookId)
    {
        if (!await context.Books.AnyAsync(book => book.Id == bookId)) return NotFound();
        if (!await context.Favorites.AnyAsync(item => item.UserId == currentUser.UserId && item.BookId == bookId))
        {
            context.Favorites.Add(new Favorite { UserId = currentUser.UserId, BookId = bookId });
            await context.SaveChangesAsync();
        }
        return NoContent();
    }

    [HttpDelete("{bookId:guid}")]
    public async Task<IActionResult> Remove(Guid bookId)
    {
        var favorite = await context.Favorites.FindAsync(currentUser.UserId, bookId);
        if (favorite is not null)
        {
            context.Favorites.Remove(favorite);
            await context.SaveChangesAsync();
        }
        return NoContent();
    }
}
