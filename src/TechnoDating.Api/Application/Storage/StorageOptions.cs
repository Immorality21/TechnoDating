namespace TechnoDating.Api.Application.Storage;

public class StorageOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string Region { get; set; } = "auto";
    public string Bucket { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; } = true;
    public int SignedUrlMinutes { get; set; } = 15;
}
