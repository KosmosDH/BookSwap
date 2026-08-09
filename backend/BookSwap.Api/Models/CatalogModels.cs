namespace BookSwap.Api.Models;

public sealed class Author
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public sealed class Genre
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
}

public sealed class BookGenre
{
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public Guid GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

public sealed class BookPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
