using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(AppDbContext context) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserSummaryDto>> Get(Guid id)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return user is null ? NotFound() : Ok(await user.ToSummaryAsync(context));
    }
}
