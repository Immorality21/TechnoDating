namespace TechnoDating.Services;

public interface ILanguageService
{
    string CurrentLanguage { get; }
    IReadOnlyList<(string Code, string DisplayName)> AvailableLanguages { get; }

    event Action? OnLanguageChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SetLanguageAsync(string languageCode);
}
