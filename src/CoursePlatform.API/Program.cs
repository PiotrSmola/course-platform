using System.Text;
using CoursePlatform.Application;
using CoursePlatform.Infrastructure;
using CoursePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Domain.Entities;
using Serilog;
using System.Threading.RateLimiting;

const string JwtKeyPlaceholder = "REPLACE_WITH_YOUR_OWN_KEY_AT_LEAST_32_CHARS";

var builder = WebApplication.CreateBuilder(args);

ValidateJwtConfiguration(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
    options.Limits.MaxRequestLineSize = 8192;
    options.Limits.MaxRequestHeadersTotalSize = 32768;
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        var jwtKey = builder.Configuration["Jwt:Key"]!;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.Append("Retry-After", "60");
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Zbyt wiele żądań. Spróbuj ponownie za chwilę.",
            statusCode = 429
        }, cancellationToken);
    };

    options.AddPolicy("auth", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("api", httpContext =>
    {
        var userId = httpContext.User.FindFirst("sub")?.Value;
        var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CoursePlatform.API.Middleware.SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CoursePlatform.API.Middleware.ExceptionHandlingMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await context.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await ApplicationDbContextSeed.SeedRolesAsync(roleManager);

        var seedEnabled = builder.Configuration.GetValue("Dev:Seed", false);
        if (app.Environment.IsDevelopment() && seedEnabled)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await ApplicationDbContextSeed.SeedAsync(context, userManager, roleManager);
        }
    }
}

app.Run();

static void ValidateJwtConfiguration(IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("Jwt:Key is not configured.");

    if (jwtKey == JwtKeyPlaceholder)
        throw new InvalidOperationException("Jwt:Key must be changed from the .env.example placeholder.");

    if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HMAC-SHA256.");

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]))
        throw new InvalidOperationException("Jwt:Issuer is not configured.");

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]))
        throw new InvalidOperationException("Jwt:Audience is not configured.");
}

public partial class Program { }
