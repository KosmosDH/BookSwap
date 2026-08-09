using BookSwap.Api.Data;
using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(AppDbContext context, UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> Stats() => Ok(new AdminStatsDto(
        await context.Users.CountAsync(),
        await context.Books.IgnoreQueryFilters().CountAsync(item => !item.IsDeleted),
        await context.ExchangeRequests.CountAsync(),
        await context.Complaints.CountAsync(item => item.Status == ComplaintStatus.Open),
        await context.ChatMessages.CountAsync()));

    [HttpGet("users")]
    public async Task<ActionResult<object>> Users() => Ok(await context.Users.AsNoTracking()
        .OrderByDescending(item => item.CreatedAt)
        .Select(item => new { item.Id, item.Email, item.DisplayName, item.City, item.IsBlocked, item.CreatedAt })
        .ToListAsync());

    [HttpPatch("users/{id:guid}/toggle-block")]
    public async Task<ActionResult<object>> ToggleBlock(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        if (await userManager.IsInRoleAsync(user, "Admin")) return BadRequest(new { message = "Нельзя заблокировать администратора." });
        user.IsBlocked = !user.IsBlocked;
        await userManager.UpdateAsync(user);
        return Ok(new { user.Id, user.IsBlocked });
    }

    [HttpGet("complaints")]
    public async Task<ActionResult<IReadOnlyList<ComplaintDto>>> Complaints()
    {
        var items = await context.Complaints.AsNoTracking()
            .Include(item => item.Reporter)
            .Include(item => item.TargetUser)
            .Include(item => item.Book)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
        var result = new List<ComplaintDto>();
        foreach (var item in items)
        {
            result.Add(new ComplaintDto(
                item.Id,
                await item.Reporter.ToSummaryAsync(context),
                item.BookId,
                item.Book?.Title,
                item.TargetUser is null ? null : await item.TargetUser.ToSummaryAsync(context),
                item.Reason,
                item.Status,
                item.AdminComment,
                item.CreatedAt));
        }
        return Ok(result);
    }

    [HttpPatch("complaints/{id:guid}")]
    public async Task<ActionResult> UpdateComplaint(Guid id, UpdateComplaintRequest request)
    {
        var complaint = await context.Complaints.FirstOrDefaultAsync(item => item.Id == id);
        if (complaint is null) return NotFound();
        complaint.Status = request.Status;
        complaint.AdminComment = request.AdminComment?.Trim();
        await context.SaveChangesAsync();
        return Ok(new { message = "Жалоба обновлена." });
    }
}
