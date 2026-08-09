using System.ComponentModel.DataAnnotations;
using BookSwap.Api.Models;

namespace BookSwap.Api.Dtos;

public sealed record CreateExchangeRequest(
    Guid RequestedBookId,
    Guid? OfferedBookId,
    [MaxLength(1000)] string? Message);

public sealed record ExchangeDto(
    Guid Id,
    UserSummaryDto Sender,
    UserSummaryDto Receiver,
    BookListItemDto RequestedBook,
    BookListItemDto? OfferedBook,
    string? Message,
    ExchangeStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool CanAccept,
    bool CanCancel,
    bool CanComplete);

public sealed record CreateReviewRequest(Guid ExchangeRequestId, [Range(1, 5)] int Rating, [Required, MaxLength(1000)] string Text);
public sealed record ReviewDto(Guid Id, UserSummaryDto Author, int Rating, string Text, DateTime CreatedAt);
public sealed record CreateComplaintRequest(Guid? BookId, Guid? TargetUserId, [Required, MaxLength(1000)] string Reason);
public sealed record ComplaintDto(Guid Id, UserSummaryDto Reporter, Guid? BookId, string? BookTitle, UserSummaryDto? TargetUser, string Reason, ComplaintStatus Status, string? AdminComment, DateTime CreatedAt);
public sealed record UpdateComplaintRequest(ComplaintStatus Status, [MaxLength(1000)] string? AdminComment);
public sealed record NotificationDto(Guid Id, NotificationType Type, string Title, string Body, string? Link, bool IsRead, DateTime CreatedAt);
public sealed record ChatMessageDto(Guid Id, UserSummaryDto Sender, UserSummaryDto Receiver, string Content, DateTime SentAt, DateTime? ReadAt);
public sealed record SendMessageRequest(Guid ReceiverId, [Required, MaxLength(2000)] string Content);
public sealed record ConversationDto(UserSummaryDto User, string LastMessage, DateTime LastMessageAt, int UnreadCount);
public sealed record AdminStatsDto(int Users, int Books, int Exchanges, int OpenComplaints, int Messages);
