using TechnoDating.Admin.Components;
using TechnoDating.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Talks to the main API as the admin (X-Admin-Key). Server-side only — the key never reaches the browser.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000";
var adminKey = builder.Configuration["Api:AdminKey"] ?? "dev-admin-key";

builder.Services.AddHttpClient<AdminApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("X-Admin-Key", adminKey);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
