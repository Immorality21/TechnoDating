namespace TechnoDating.Contracts;

public record PhotoDto(
    Guid Id,
    int Ordinal,
    bool IsPrimary,
    string ThumbUrl,
    string CardUrl,
    string FullUrl);
