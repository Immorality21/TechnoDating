var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "TechnoDatingDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

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
