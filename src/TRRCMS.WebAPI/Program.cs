using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TRRCMS.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

app.UseMiddleware<TRRCMS.WebAPI.Middleware.GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TRRCMS.WebAPI.Middleware.MustChangePasswordMiddleware>();
app.UseStaticFiles();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
