using System.Globalization;

namespace TechnoDating.Services;

public class LanguageService : ILanguageService
{
    private const string StorageKey = "tn_language";
    private const string DefaultLanguage = "en";

    private static readonly (string Code, string DisplayName)[] Supported =
    [
        ("en", "English"),
        ("nl", "Nederlands"),
    ];

    private string _current = DefaultLanguage;

    public string CurrentLanguage => _current;

    public IReadOnlyList<(string Code, string DisplayName)> AvailableLanguages => Supported;

    public event Action? OnLanguageChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var stored = await SecureStorage.Default.GetAsync(StorageKey);
        var code = NormalizeOrFallback(stored);
        ApplyCulture(code);
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        var code = NormalizeOrFallback(languageCode);
        if (code == _current)
        {
            return;
        }
        ApplyCulture(code);
        await SecureStorage.Default.SetAsync(StorageKey, code);
        OnLanguageChanged?.Invoke();
    }

    private void ApplyCulture(string code)
    {
        _current = code;
        var culture = new CultureInfo(code);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static string NormalizeOrFallback(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return DefaultLanguage;
        }
        var lower = code.Trim().ToLowerInvariant();
        var prefix = lower.Length >= 2 ? lower[..2] : lower;
        return Supported.Any(x => x.Code == prefix) ? prefix : DefaultLanguage;
    }
}
