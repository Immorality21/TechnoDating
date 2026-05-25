using Microsoft.Extensions.Logging;

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

            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(ApiBaseAddress),
                Timeout = TimeSpan.FromSeconds(10),
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
