using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "TechnoDatingDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("TechnoDating")
    ?? throw new InvalidOperationException("Missing connection string 'TechnoDating'.");

builder.Services.AddDbContext<TechnoDatingDbContext>(options =>
{
    options.UseNpgsql(connectionString, npg => npg.UseNetTopologySuite());
});

builder.Services.AddHostedService<DatabaseInitializer>();

builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.MapGet("/", () => Results.Ok(new { service = "TechnoDating.Api", status = "ok" }));
app.MapControllers();

app.Run();
