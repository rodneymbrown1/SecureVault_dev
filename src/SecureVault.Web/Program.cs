using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureVault.Web.Components;
using SecureVault.Web.Data;
using SecureVault.Web.Data.Models;
using SecureVault.Web.Services;
using SecureVault.Web.Services.Encryption;

var builder = WebApplication.CreateBuilder(args);

// Database - PostgreSQL via DATABASE_URL (Heroku) or ConnectionStrings:DefaultConnection
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Heroku DATABASE_URL format: postgres://user:password@host:port/database
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=securevault;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
});

// Encryption services
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<IEncryptionService, DesEncryptionService>();
builder.Services.AddSingleton<IEncryptionService, TripleDesEncryptionService>();
builder.Services.AddSingleton<IEncryptionService, RsaEncryptionService>();
builder.Services.AddSingleton<IEncryptionService, EccEncryptionService>();
builder.Services.AddSingleton<EncryptionServiceFactory>();

// AWS S3
var awsRegion = builder.Configuration["AWS_REGION"] ?? builder.Configuration["AWS:Region"] ?? "us-east-1";
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var accessKey = builder.Configuration["AWS_ACCESS_KEY_ID"] ?? builder.Configuration["AWS:AccessKeyId"];
    var secretKey = builder.Configuration["AWS_SECRET_ACCESS_KEY"] ?? builder.Configuration["AWS:SecretAccessKey"];

    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
    {
        return new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));
    }
    // Falls back to default credentials (IAM role, env vars, etc.)
    return new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(awsRegion));
});

builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();
builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Heroku provides PORT env var - listen on it
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Clear();
    app.Urls.Add($"http://+:{port}");
}

app.UseHttpsRedirection();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Logout endpoint
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
