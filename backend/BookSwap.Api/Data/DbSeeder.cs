using BookSwap.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookSwap.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await context.Database.EnsureCreatedAsync();
                break;
            }
            catch (Exception ex) when (attempt < 10)
            {
                logger.LogWarning(ex, "Database is unavailable, retry {Attempt}/10", attempt);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        foreach (var role in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin@bookswap.local", "Admin123", "Администратор", "Москва", "Admin");
        var anna = await EnsureUserAsync(userManager, "anna@bookswap.local", "User123", "Анна Смирнова", "Казань", "User");
        var maxim = await EnsureUserAsync(userManager, "maxim@bookswap.local", "User123", "Максим Волков", "Санкт-Петербург", "User");
        var elena = await EnsureUserAsync(userManager, "elena@bookswap.local", "User123", "Елена Орлова", "Москва", "User");

        if (!await context.Genres.AnyAsync())
        {
            context.Genres.AddRange(
                new Genre { Name = "Научная фантастика", Slug = "science-fiction" },
                new Genre { Name = "Классика", Slug = "classics" },
                new Genre { Name = "Фэнтези", Slug = "fantasy" },
                new Genre { Name = "Научно-популярная", Slug = "popular-science" },
                new Genre { Name = "Программирование", Slug = "programming" },
                new Genre { Name = "Детектив", Slug = "detective" },
                new Genre { Name = "Психология", Slug = "psychology" },
                new Genre { Name = "История", Slug = "history" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Books.AnyAsync())
        {
            var genres = await context.Genres.ToDictionaryAsync(item => item.Slug);
            var seeds = new[]
            {
                new { Owner = anna, Title = "Солярис", Author = "Станислав Лем", Desc = "Философская научная фантастика о контакте с непостижимым разумом.", City = "Казань", Cover = "/covers/solaris.svg", Genre = "science-fiction", Condition = BookCondition.Excellent, Deal = DealType.Exchange },
                new { Owner = maxim, Title = "1984", Author = "Джордж Оруэлл", Desc = "Антиутопия о власти, языке и свободе личности.", City = "Санкт-Петербург", Cover = "/covers/1984.svg", Genre = "classics", Condition = BookCondition.Good, Deal = DealType.Exchange },
                new { Owner = elena, Title = "Чистый код", Author = "Роберт Мартин", Desc = "Практическое руководство по написанию понятного и поддерживаемого кода.", City = "Москва", Cover = "/covers/clean-code.svg", Genre = "programming", Condition = BookCondition.Good, Deal = DealType.Lend },
                new { Owner = anna, Title = "Краткая история времени", Author = "Стивен Хокинг", Desc = "Доступный рассказ о космологии, чёрных дырах и природе времени.", City = "Казань", Cover = "/covers/time.svg", Genre = "popular-science", Condition = BookCondition.Fair, Deal = DealType.GiveAway },
                new { Owner = maxim, Title = "Властелин колец", Author = "Дж. Р. Р. Толкин", Desc = "Эпическое путешествие через Средиземье.", City = "Санкт-Петербург", Cover = "/covers/lotr.svg", Genre = "fantasy", Condition = BookCondition.Excellent, Deal = DealType.Exchange },
                new { Owner = elena, Title = "Убийство в Восточном экспрессе", Author = "Агата Кристи", Desc = "Классический детектив с Эркюлем Пуаро.", City = "Москва", Cover = "/covers/orient.svg", Genre = "detective", Condition = BookCondition.Good, Deal = DealType.Sell }
            };

            foreach (var seed in seeds)
            {
                var author = await context.Authors.FirstOrDefaultAsync(item => item.Name == seed.Author);
                if (author is null)
                {
                    author = new Author { Name = seed.Author };
                    context.Authors.Add(author);
                }

                var book = new Book
                {
                    OwnerId = seed.Owner.Id,
                    Title = seed.Title,
                    Author = author,
                    Description = seed.Desc,
                    City = seed.City,
                    Condition = seed.Condition,
                    DealType = seed.Deal,
                    Price = seed.Deal == DealType.Sell ? 450 : null,
                    Language = "Русский",
                    Photos = new List<BookPhoto> { new() { Url = seed.Cover, IsPrimary = true } },
                    BookGenres = new List<BookGenre> { new() { GenreId = genres[seed.Genre].Id } }
                };
                context.Books.Add(book);
            }
            await context.SaveChangesAsync();
        }

        var solaris = await context.Books.FirstAsync(book => book.Title == "Солярис");
        var nineteenEightyFour = await context.Books.FirstAsync(book => book.Title == "1984");
        var cleanCode = await context.Books.FirstAsync(book => book.Title == "Чистый код");
        var lordOfTheRings = await context.Books.FirstAsync(book => book.Title == "Властелин колец");

        if (!await context.Favorites.AnyAsync())
        {
            context.Favorites.AddRange(
                new Favorite { UserId = anna.Id, BookId = lordOfTheRings.Id },
                new Favorite { UserId = elena.Id, BookId = solaris.Id });
        }

        if (!await context.ExchangeRequests.AnyAsync())
        {
            context.ExchangeRequests.Add(new ExchangeRequest
            {
                SenderId = maxim.Id,
                ReceiverId = anna.Id,
                RequestedBookId = solaris.Id,
                OfferedBookId = nineteenEightyFour.Id,
                Message = "Здравствуйте! Предлагаю обменяться на «1984». Могу встретиться в центре на выходных.",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-5)
            });
        }

        if (!await context.ChatMessages.AnyAsync())
        {
            context.ChatMessages.AddRange(
                new ChatMessage
                {
                    SenderId = maxim.Id,
                    ReceiverId = anna.Id,
                    Content = "Здравствуйте! Увидел у вас «Солярис». Книга ещё доступна?",
                    SentAt = DateTime.UtcNow.AddHours(-6),
                    ReadAt = DateTime.UtcNow.AddHours(-5.5)
                },
                new ChatMessage
                {
                    SenderId = anna.Id,
                    ReceiverId = maxim.Id,
                    Content = "Да, доступна. Я как раз ищу хорошую антиутопию для обмена.",
                    SentAt = DateTime.UtcNow.AddHours(-5.5),
                    ReadAt = DateTime.UtcNow.AddHours(-5)
                },
                new ChatMessage
                {
                    SenderId = maxim.Id,
                    ReceiverId = anna.Id,
                    Content = "Тогда отправил предложение с «1984» 🙂",
                    SentAt = DateTime.UtcNow.AddHours(-5)
                },
                new ChatMessage
                {
                    SenderId = elena.Id,
                    ReceiverId = anna.Id,
                    Content = "Анна, подскажите, пожалуйста, где вам удобно передать книгу?",
                    SentAt = DateTime.UtcNow.AddDays(-1),
                    ReadAt = DateTime.UtcNow.AddHours(-20)
                });
        }

        if (!await context.Notifications.AnyAsync())
        {
            context.Notifications.AddRange(
                new Notification
                {
                    RecipientId = anna.Id,
                    Type = NotificationType.Exchange,
                    Title = "Новая заявка на обмен",
                    Body = "Максим предлагает «1984» в обмен на «Солярис».",
                    Link = "/dashboard?tab=exchanges",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new Notification
                {
                    RecipientId = anna.Id,
                    Type = NotificationType.Message,
                    Title = "Новое сообщение",
                    Body = "Максим: Тогда отправил предложение с «1984» 🙂",
                    Link = $"/chat/{maxim.Id}",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new Notification
                {
                    RecipientId = maxim.Id,
                    Type = NotificationType.System,
                    Title = "Добро пожаловать в BookSwap",
                    Body = "Добавляйте книги, находите читателей и договаривайтесь об обмене в чате.",
                    Link = "/catalog",
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                });
        }

        await context.SaveChangesAsync();

        logger.LogInformation("BookSwap database seed completed. Admin id: {AdminId}", admin.Id);
    }

    private static async Task<AppUser> EnsureUserAsync(
        UserManager<AppUser> userManager,
        string email,
        string password,
        string displayName,
        string city,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
                City = city
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}
