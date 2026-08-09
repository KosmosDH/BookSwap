namespace BookSwap.Api.Models;

public sealed class Favorite
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ExchangeRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;
    public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = null!;
    public Guid RequestedBookId { get; set; }
    public Book RequestedBook { get; set; } = null!;
    public Guid? OfferedBookId { get; set; }
    public Book? OfferedBook { get; set; }
    public string? Message { get; set; }
    public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    public Guid TargetUserId { get; set; }
    public AppUser TargetUser { get; set; } = null!;
    public Guid ExchangeRequestId { get; set; }
    public ExchangeRequest ExchangeRequest { get; set; } = null!;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;
    public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}

public sealed class Complaint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReporterId { get; set; }
    public AppUser Reporter { get; set; } = null!;
    public Guid? TargetUserId { get; set; }
    public AppUser? TargetUser { get; set; }
    public Guid? BookId { get; set; }
    public Book? Book { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipientId { get; set; }
    public AppUser Recipient { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
