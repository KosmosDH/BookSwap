using BookSwap.Api.Dtos;
using BookSwap.Api.Models;
using BookSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookSwap.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<AppUser> userManager, JwtTokenService jwtTokenService, CurrentUserService currentUser) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email.Trim()) is not null)
            return Conflict(new { message = "Пользователь с таким email уже существует." });

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim(),
            City = request.City.Trim(),
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = "Не удалось создать пользователя.", errors = result.Errors.Select(error => error.Description) });

        await userManager.AddToRoleAsync(user, "User");
        return Ok(await CreateResponseAsync(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Неверный email или пароль." });
        if (user.IsBlocked)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Аккаунт заблокирован администратором." });

        return Ok(await CreateResponseAsync(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
        return user is null ? Unauthorized() : Ok(await ToProfileAsync(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
        if (user is null) return Unauthorized();
        user.DisplayName = request.DisplayName.Trim();
        user.City = request.City.Trim();
        user.Bio = request.Bio?.Trim();
        user.AvatarUrl = request.AvatarUrl?.Trim();
        await userManager.UpdateAsync(user);
        return Ok(await ToProfileAsync(user));
    }

    private async Task<AuthResponse> CreateResponseAsync(AppUser user)
    {
        var (token, expiresAt) = await jwtTokenService.CreateAsync(user);
        return new AuthResponse(token, expiresAt, await ToProfileAsync(user));
    }

    private async Task<UserProfileDto> ToProfileAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserProfileDto(user.Id, user.Email ?? string.Empty, user.DisplayName, user.City, user.AvatarUrl, user.Bio, roles.ToArray(), user.IsBlocked);
    }
}
