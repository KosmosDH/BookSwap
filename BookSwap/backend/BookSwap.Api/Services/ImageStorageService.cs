using System.Net.Http.Headers;
using System.Text.Json;

namespace BookSwap.Api.Services;

public interface IImageStorage
{
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed class ImageStorageService(
    IWebHostEnvironment environment,
    IConfiguration configuration) : IImageStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length is 0 or > 5 * 1024 * 1024)
            throw new ArgumentException("Файл должен быть не больше 5 МБ.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Поддерживаются JPG, PNG и WEBP.");

        var cloudName = configuration["Cloudinary:CloudName"];
        var uploadPreset = configuration["Cloudinary:UploadPreset"];
        if (!string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(uploadPreset))
        {
            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(content, "file", file.FileName);
            form.Add(new StringContent(uploadPreset), "upload_preset");
            var response = await client.PostAsync($"https://api.cloudinary.com/v1_1/{cloudName}/image/upload", form, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return document.RootElement.GetProperty("secure_url").GetString()
                ?? throw new InvalidOperationException("Cloudinary did not return an image URL.");
        }

        var folder = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);
        await using var output = File.Create(path);
        await file.CopyToAsync(output, cancellationToken);
        return $"/uploads/{fileName}";
    }
}
