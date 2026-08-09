using BookSwap.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<BookGenre> BookGenres => Set<BookGenre>();
    public DbSet<BookPhoto> BookPhotos => Set<BookPhoto>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ExchangeRequest> ExchangeRequests => Set<ExchangeRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Book>().HasQueryFilter(book => !book.IsDeleted);
        builder.Entity<Book>().Property(book => book.Price).HasPrecision(10, 2);
        builder.Entity<Book>().HasIndex(book => book.Title);
        builder.Entity<Book>().HasIndex(book => new { book.City, book.Status });
        builder.Entity<Author>().HasIndex(author => author.Name).IsUnique();
        builder.Entity<Genre>().HasIndex(genre => genre.Slug).IsUnique();

        builder.Entity<Book>()
            .HasOne(book => book.Owner)
            .WithMany(user => user.Books)
            .HasForeignKey(book => book.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BookGenre>().HasKey(item => new { item.BookId, item.GenreId });
        builder.Entity<BookGenre>()
            .HasOne(item => item.Book)
            .WithMany(book => book.BookGenres)
            .HasForeignKey(item => item.BookId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<BookGenre>()
            .HasOne(item => item.Genre)
            .WithMany(genre => genre.BookGenres)
            .HasForeignKey(item => item.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>().HasKey(item => new { item.UserId, item.BookId });
        builder.Entity<Favorite>()
            .HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Favorite>()
            .HasOne(item => item.Book)
            .WithMany()
            .HasForeignKey(item => item.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ExchangeRequest>()
            .HasOne(item => item.Sender)
            .WithMany()
            .HasForeignKey(item => item.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ExchangeRequest>()
            .HasOne(item => item.Receiver)
            .WithMany()
            .HasForeignKey(item => item.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ExchangeRequest>()
            .HasOne(item => item.RequestedBook)
            .WithMany()
            .HasForeignKey(item => item.RequestedBookId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ExchangeRequest>()
            .HasOne(item => item.OfferedBook)
            .WithMany()
            .HasForeignKey(item => item.OfferedBookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>().HasIndex(item => new { item.AuthorId, item.ExchangeRequestId }).IsUnique();
        builder.Entity<Review>()
            .HasOne(item => item.Author)
            .WithMany()
            .HasForeignKey(item => item.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Review>()
            .HasOne(item => item.TargetUser)
            .WithMany()
            .HasForeignKey(item => item.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>().HasIndex(item => new { item.SenderId, item.ReceiverId, item.SentAt });
        builder.Entity<ChatMessage>()
            .HasOne(item => item.Sender)
            .WithMany()
            .HasForeignKey(item => item.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ChatMessage>()
            .HasOne(item => item.Receiver)
            .WithMany()
            .HasForeignKey(item => item.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Complaint>()
            .HasOne(item => item.Reporter)
            .WithMany()
            .HasForeignKey(item => item.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Complaint>()
            .HasOne(item => item.TargetUser)
            .WithMany()
            .HasForeignKey(item => item.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>().HasIndex(item => new { item.RecipientId, item.IsRead, item.CreatedAt });
    }
}
