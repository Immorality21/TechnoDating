using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TechnoDating.Services;

namespace TechnoDating
{
    public static class MauiProgram
    {
#if ANDROID
        // Android emulator routes its loopback to the host machine via 10.0.2.2.
        private const string ApiBaseAddress = "http://10.0.2.2:5000";
#else
        private const string ApiBaseAddress = "http://localhost:5000";
#endif

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddLocalization();

            builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
            builder.Services.AddSingleton<ILanguageService, LanguageService>();
            builder.Services.AddScoped<AuthenticationStateProvider, TechnoDatingAuthenticationStateProvider>();
            builder.Services.AddTransient<AuthMessageHandler>();

            builder.Services.AddHttpClient("auth", client =>
            {
                client.BaseAddress = new Uri(ApiBaseAddress);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddHttpClient("api", client =>
            {
                client.BaseAddress = new Uri(ApiBaseAddress);
                client.Timeout = TimeSpan.FromSeconds(10);
            }).AddHttpMessageHandler<AuthMessageHandler>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
