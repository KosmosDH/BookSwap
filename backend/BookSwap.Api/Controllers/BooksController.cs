using System.Text.Json;
using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BookSwap.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(AppDbContext context, CurrentUserService currentUser, IDistributedCache cache) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookListItemDto>>> GetAll(
        string? search,
        Guid? genreId,
        string? city,
        BookCondition? condition,
        DealType? dealType,
        string sort = "newest",
        int page = 1,
        int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var query = BaseQuery().Where(book => book.Status == BookStatus.Available);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(book => book.Title.ToLower().Contains(term) || book.Author.Name.ToLower().Contains(term) || book.Description.ToLower().Contains(term));
        }
        if (genreId.HasValue) query = query.Where(book => book.BookGenres.Any(item => item.GenreId == genreId));
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(book => book.City.ToLower() == city.Trim().ToLower());
        if (condition.HasValue) query = query.Where(book => book.Condition == condition);
        if (dealType.HasValue) query = query.Where(book => book.DealType == dealType);

        query = sort switch
        {
            "oldest" => query.OrderBy(book => book.CreatedAt),
            "title" => query.OrderBy(book => book.Title),
            _ => query.OrderByDescending(book => book.CreatedAt)
        };

        var total = await query.CountAsync();
        var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = new List<BookListItemDto>();
        foreach (var book in books) items.Add(await book.ToListItemAsync(context, currentUser.UserId));
        return Ok(new PagedResult<BookListItemDto>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize)));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> Featured()
    {
        const string key = "books:featured:v1";
        if (!currentUser.IsAuthenticated)
        {
            var cached = await cache.GetStringAsync(key);
            if (cached is not null)
                return Ok(JsonSerializer.Deserialize<List<BookListItemDto>>(cached));
        }

        var books = await BaseQuery().Where(book => book.Status == BookStatus.Available).OrderByDescending(book => book.CreatedAt).Take(6).ToListAsync();
        var result = new List<BookListItemDto>();
        foreach (var book in books) result.Add(await book.ToListItemAsync(context, currentUser.UserId));
        if (!currentUser.IsAuthenticated)
            await cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) });
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDetailsDto>> Get(Guid id)
    {
        var book = await BaseQuery().FirstOrDefaultAsync(item => item.Id == id);
        if (book is null) return NotFound(new { message = "Книга не найдена." });
        var owner = await book.Owner.ToSummaryAsync(context);
        var favorite = currentUser.IsAuthenticated && await context.Favorites.AnyAsync(item => item.UserId == currentUser.UserId && item.BookId == id);
        return Ok(new BookDetailsDto(
            book.Id, book.Title, book.Description, book.Author.Name, book.Isbn, book.Language, book.City,
            book.Condition, book.DealType, book.Status, book.Price, book.CreatedAt, book.UpdatedAt, owner,
            book.BookGenres.Select(item => new GenreDto(item.Genre.Id, item.Genre.Name, item.Genre.Slug)).ToArray(),
            book.Photos.OrderByDescending(item => item.IsPrimary).Select(item => new BookPhotoDto(item.Id, item.Url, item.IsPrimary)).ToArray(),
            favorite, currentUser.UserId == book.OwnerId || User.IsInRole("Admin")));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> Mine()
    {
        var books = await BaseQuery().Where(book => book.OwnerId == currentUser.UserId).OrderByDescending(book => book.CreatedAt).ToListAsync();
        var result = new List<BookListItemDto>();
        foreach (var book in books) result.Add(await book.ToListItemAsync(context, currentUser.UserId));
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookDetailsDto>> Create(CreateBookRequest request)
    {
        var validation = ValidatePrice(request.DealType, request.Price);
        if (validation is not null) return BadRequest(new { message = validation });
        var author = await FindOrCreateAuthorAsync(request.Author);
        var validGenres = await context.Genres.Where(item => request.GenreIds.Contains(item.Id)).Select(item => item.Id).ToListAsync();
        var book = new Book
        {
            OwnerId = currentUser.UserId,
            Title = request.Title.Trim(),
            Author = author,
            Description = request.Description.Trim(),
            Isbn = request.Isbn?.Trim(),
            Language = request.Language.Trim(),
            City = request.City.Trim(),
            Condition = request.Condition,
            DealType = request.DealType,
            Price = request.DealType == DealType.Sell ? request.Price : null,
            Photos = request.PhotoUrls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().Take(5).Select((url, index) => new BookPhoto { Url = url, IsPrimary = index == 0 }).ToList(),
            BookGenres = validGenres.Select(id => new BookGenre { GenreId = id }).ToList()
        };
        context.Books.Add(book);
        await context.SaveChangesAsync();
        await cache.RemoveAsync("books:featured:v1");
        return CreatedAtAction(nameof(Get), new { id = book.Id }, await GetCreatedDetailsAsync(book.Id));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookDetailsDto>> Update(Guid id, UpdateBookRequest request)
    {
        var book = await BaseQuery(tracked: true).FirstOrDefaultAsync(item => item.Id == id);
        if (book is null) return NotFound();
        if (book.OwnerId != currentUser.UserId && !User.IsInRole("Admin")) return Forbid();
        var validation = ValidatePrice(request.DealType, request.Price);
        if (validation is not null) return BadRequest(new { message = validation });

        book.Title = request.Title.Trim();
        book.Author = await FindOrCreateAuthorAsync(request.Author);
        book.Description = request.Description.Trim();
        book.Isbn = request.Isbn?.Trim();
        book.Language = request.Language.Trim();
        book.City = request.City.Trim();
        book.Condition = request.Condition;
        book.DealType = request.DealType;
        book.Status = request.Status;
        book.Price = request.DealType == DealType.Sell ? request.Price : null;
        book.UpdatedAt = DateTime.UtcNow;

        context.BookGenres.RemoveRange(book.BookGenres);
        var validGenres = await context.Genres.Where(item => request.GenreIds.Contains(item.Id)).Select(item => item.Id).ToListAsync();
        book.BookGenres = validGenres.Select(genreId => new BookGenre { BookId = book.Id, GenreId = genreId }).ToList();
        context.BookPhotos.RemoveRange(book.Photos);
        book.Photos = request.PhotoUrls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().Take(5).Select((url, index) => new BookPhoto { BookId = book.Id, Url = url, IsPrimary = index == 0 }).ToList();

        await context.SaveChangesAsync();
        await cache.RemoveAsync("books:featured:v1");
        return Ok(await GetCreatedDetailsAsync(book.Id));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await context.Books.FirstOrDefaultAsync(item => item.Id == id);
        if (book is null) return NotFound();
        if (book.OwnerId != currentUser.UserId && !User.IsInRole("Admin")) return Forbid();
        book.IsDeleted = true;
        book.Status = BookStatus.Hidden;
        book.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        await cache.RemoveAsync("books:featured:v1");
        return NoContent();
    }

    private IQueryable<Book> BaseQuery(bool tracked = false)
    {
        IQueryable<Book> query = context.Books
            .Include(book => book.Owner)
            .Include(book => book.Author)
            .Include(book => book.Photos)
            .Include(book => book.BookGenres).ThenInclude(item => item.Genre);
        return tracked ? query : query.AsNoTracking();
    }

    private async Task<Author> FindOrCreateAuthorAsync(string name)
    {
        var trimmed = name.Trim();
        var author = await context.Authors.FirstOrDefaultAsync(item => item.Name.ToLower() == trimmed.ToLower());
        if (author is not null) return author;
        author = new Author { Name = trimmed };
        context.Authors.Add(author);
        return author;
    }

    private static string? ValidatePrice(DealType dealType, decimal? price) =>
        dealType == DealType.Sell && (!price.HasValue || price <= 0) ? "Для продажи необходимо указать положительную цену." : null;

    private async Task<BookDetailsDto> GetCreatedDetailsAsync(Guid id)
    {
        var action = await Get(id);
        return (BookDetailsDto)((ObjectResult)action.Result!).Value!;
    }
}
