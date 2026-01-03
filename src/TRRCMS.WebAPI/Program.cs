using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Mappings;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============== DATABASE ==============
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// ============== REPOSITORIES ==============
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();

// ============== MEDIATOR ==============
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(TRRCMS.Application.Buildings.Commands.CreateBuilding.CreateBuildingCommand).Assembly));

// ============== AUTOMAPPER ==============
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ============== CONTROLLERS ==============
builder.Services.AddControllers();

// ============== SWAGGER ==============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "TRRCMS API",
        Version = "v1",
        Description = "UN-Habitat Tenure Rights Registration & Claims Management System"
    });
});

// ============== CORS (for development) ==============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============== MIDDLEWARE ==============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();