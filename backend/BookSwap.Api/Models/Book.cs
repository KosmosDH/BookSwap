namespace BookSwap.Api.Models;

public sealed class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Language { get; set; } = "Русский";
    public string City { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public BookCondition Condition { get; set; }
    public DealType DealType { get; set; }
    public BookStatus Status { get; set; } = BookStatus.Available;
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public ICollection<BookPhoto> Photos { get; set; } = new List<BookPhoto>();
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
}
