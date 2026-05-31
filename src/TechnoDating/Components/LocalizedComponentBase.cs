using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TechnoDating.Resources;
using TechnoDating.Services;

namespace TechnoDating.Components;

// Inherit from this in any page that uses @L["..."]. It re-renders the
// page whenever the language changes (so labels swap immediately).
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    [Inject] protected ILanguageService Language { get; set; } = default!;
    [Inject] protected IStringLocalizer<Strings> L { get; set; } = default!;

    protected override void OnInitialized()
    {
        Language.OnLanguageChanged += HandleLanguageChanged;
    }

    private void HandleLanguageChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        Language.OnLanguageChanged -= HandleLanguageChanged;
        GC.SuppressFinalize(this);
    }
}
