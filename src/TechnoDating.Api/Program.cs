using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechnoDating.Api.Application.Auth;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
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

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<TechnoDatingDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<OtpOptions>(builder.Configuration.GetSection("Otp"));

builder.Services.AddSingleton<IPasswordHasher<OtpChallenge>, PasswordHasher<OtpChallenge>>();
builder.Services.AddScoped<IOtpSender, ConsoleOtpSender>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
if (string.IsNullOrWhiteSpace(jwt.SigningKey))
{
    throw new InvalidOperationException("Missing 'Jwt:SigningKey'. Set it in appsettings.Development.json for dev, or env var/Key Vault in prod.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var idClaim = context.Principal?.FindFirst("sub")?.Value;
                var stampClaim = context.Principal?.FindFirst("security_stamp")?.Value;

                if (!Guid.TryParse(idClaim, out var userId) || string.IsNullOrEmpty(stampClaim))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                {
                    context.Fail("User not found.");
                    return;
                }

                var current = await userManager.GetSecurityStampAsync(user);
                if (!string.Equals(current, stampClaim, StringComparison.Ordinal))
                {
                    context.Fail("Security stamp mismatch.");
                    return;
                }

                if (context.Principal!.Identity is ClaimsIdentity identity && identity.FindFirst(ClaimTypes.NameIdentifier) is null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                }
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHostedService<DatabaseInitializer>();

builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { service = "TechnoDating.Api", status = "ok" }));
app.MapControllers();

app.Run();
