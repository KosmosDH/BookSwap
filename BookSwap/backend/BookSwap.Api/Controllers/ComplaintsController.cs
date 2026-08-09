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
[Route("api/complaints")]
public sealed class ComplaintsController(AppDbContext context, CurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create(CreateComplaintRequest request)
    {
        if (!request.BookId.HasValue && !request.TargetUserId.HasValue)
            return BadRequest(new { message = "Нужно указать книгу или пользователя." });
        if (request.TargetUserId == currentUser.UserId)
            return BadRequest(new { message = "Нельзя пожаловаться на самого себя." });
        if (request.BookId.HasValue && !await context.Books.AnyAsync(item => item.Id == request.BookId)) return NotFound(new { message = "Книга не найдена." });
        if (request.TargetUserId.HasValue && !await context.Users.AnyAsync(item => item.Id == request.TargetUserId)) return NotFound(new { message = "Пользователь не найден." });

        context.Complaints.Add(new Complaint
        {
            ReporterId = currentUser.UserId,
            BookId = request.BookId,
            TargetUserId = request.TargetUserId,
            Reason = request.Reason.Trim()
        });
        await context.SaveChangesAsync();
        return Ok(new { message = "Жалоба отправлена модератору." });
    }
}
