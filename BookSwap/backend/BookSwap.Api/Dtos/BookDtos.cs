using System.ComponentModel.DataAnnotations;
using BookSwap.Api.Models;

namespace BookSwap.Api.Dtos;

public sealed record GenreDto(Guid Id, string Name, string Slug);
public sealed record BookPhotoDto(Guid Id, string Url, bool IsPrimary);

public sealed record BookListItemDto(
    Guid Id,
    string Title,
    string Author,
    string City,
    BookCondition Condition,
    DealType DealType,
    BookStatus Status,
    decimal? Price,
    string? CoverUrl,
    DateTime CreatedAt,
    UserSummaryDto Owner,
    string[] Genres,
    bool IsFavorite);

public sealed record BookDetailsDto(
    Guid Id,
    string Title,
    string Description,
    string Author,
    string? Isbn,
    string Language,
    string City,
    BookCondition Condition,
    DealType DealType,
    BookStatus Status,
    decimal? Price,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    UserSummaryDto Owner,
    GenreDto[] Genres,
    BookPhotoDto[] Photos,
    bool IsFavorite,
    bool CanEdit);

public sealed record CreateBookRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(120)] string Author,
    [Required, MaxLength(2500)] string Description,
    [MaxLength(20)] string? Isbn,
    [Required, MaxLength(50)] string Language,
    [Required, MaxLength(80)] string City,
    BookCondition Condition,
    DealType DealType,
    decimal? Price,
    Guid[] GenreIds,
    string[] PhotoUrls);

public sealed record UpdateBookRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(120)] string Author,
    [Required, MaxLength(2500)] string Description,
    [MaxLength(20)] string? Isbn,
    [Required, MaxLength(50)] string Language,
    [Required, MaxLength(80)] string City,
    BookCondition Condition,
    DealType DealType,
    BookStatus Status,
    decimal? Price,
    Guid[] GenreIds,
    string[] PhotoUrls);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
