namespace TechnoDating.Api.Infrastructure.Entities;

public class Photo
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Ordinal { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/webp";
    public string ModerationStatus { get; set; } = "approved";
    public DateTimeOffset UploadedAt { get; set; }

    public User? User { get; set; }
}
