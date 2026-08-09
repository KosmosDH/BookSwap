using System.ComponentModel.DataAnnotations;

namespace BookSwap.Api.Dtos;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MaxLength(80)] string DisplayName,
    [Required, MaxLength(80)] string City);

public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record UserSummaryDto(Guid Id, string DisplayName, string City, string? AvatarUrl, double Rating, int ReviewsCount);
public sealed record AuthResponse(string Token, DateTime ExpiresAt, UserProfileDto User);
public sealed record UserProfileDto(Guid Id, string Email, string DisplayName, string City, string? AvatarUrl, string? Bio, string[] Roles, bool IsBlocked);
public sealed record UpdateProfileRequest([Required, MaxLength(80)] string DisplayName, [Required, MaxLength(80)] string City, [MaxLength(500)] string? Bio, string? AvatarUrl);
