using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Route("api/genres")]
public sealed class GenresController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAll() => Ok(await context.Genres
        .AsNoTracking()
        .OrderBy(item => item.Name)
        .Select(item => new GenreDto(item.Id, item.Name, item.Slug))
        .ToListAsync());
}
