using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TRRCMS.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Forwarded-headers config ─────────────────────────────────────
// When running behind a reverse proxy (ngrok, nginx, Traefik, k8s ingress) the proxy
// terminates TLS and forwards plain HTTP to our container with the original scheme/IP
// preserved in X-Forwarded-Proto / X-Forwarded-For. Without UseForwardedHeaders the app
// sees scheme=http and RemoteIp=<proxy>, which:
//   - breaks IpFilterMiddleware (it would block real clients while letting the proxy through)
//   - breaks any generated URLs / scheme-sensitive logic
//   - can cause infinite HTTPS-redirect loops with reverse proxies that re-issue HTTPS
//
// KnownNetworks / KnownProxies are cleared because ngrok and dynamic-IP proxies don't
// have stable upstream IPs. Trust is therefore enforced by the deployment (only expose
// 8080 to the trusted proxy network), not by IP allowlist here.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ── Services ─────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500L * 1024 * 1024; // 500 MB
});

builder.Services.AddControllers();

// ── Localization (Arabic/English error messages via Accept-Language header) ──
// NOTE: Do NOT set ResourcesPath here. The ErrorMessages marker class lives at
// namespace TRRCMS.WebAPI (flat), and has a sibling ErrorMessages.cs next to
// Resources/ErrorMessages.resx. The .NET SDK's EmbeddedResourceUseDependentUponConvention
// embeds the .resx under the class's namespace — TRRCMS.WebAPI.ErrorMessages.resources —
// not the folder path. Setting ResourcesPath = "Resources" would make
// ResourceManagerStringLocalizerFactory probe a different stream name that does not
// exist, causing every IStringLocalizer<ErrorMessages> lookup to silently fall back to
// returning the key name (e.g. "Title_ValidationFailed") instead of the resolved value.
builder.Services.AddLocalization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TRRCMS API",
        Version = "v1",
        Description = "UN-Habitat Tenure Rights Registration & Claims Management System"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}
else
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured"),
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" });

var app = builder.Build();

// ── Reporting subsystem bootstrap (QuestPDF license + Arabic font) ──
TRRCMS.Infrastructure.Reporting.ReportingBootstrap.Initialize(
    app.Services,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Reporting"));

// ── Seeding ──────────────────────────────────────────────────────
await app.SeedDatabaseAsync();

// ── Middleware ────────────────────────────────────────────────────
// IMPORTANT: UseRequestLocalization must run BEFORE GlobalExceptionHandlingMiddleware
// so that CultureInfo.CurrentUICulture is set from the Accept-Language header before
// any exception bubbles up to the global handler's localization logic.
app.UseRequestLocalization(options =>
{
    var supportedCultures = new[] { "en", "ar" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

// ForwardedHeaders must run before anything that reads Request.Scheme, Request.IsHttps,
// or Connection.RemoteIpAddress — otherwise it reads stale (proxy) values.
app.UseForwardedHeaders();

app.UseMiddleware<TRRCMS.WebAPI.Middleware.GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseHttpsRedirection();
// IP filter runs BEFORE authentication so banned/non-allowlisted IPs can't even attempt to authenticate.
// Loopback is always allowed (see middleware) so local dev / health checks aren't bricked by misconfig.
app.UseMiddleware<TRRCMS.WebAPI.Middleware.IpFilterMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TRRCMS.WebAPI.Middleware.MustChangePasswordMiddleware>();
app.UseStaticFiles();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
