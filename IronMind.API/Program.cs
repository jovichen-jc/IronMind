using System.Text;
using IronMind.API.Routes;
using IronMind.Core.Interfaces;
using IronMind.Data;
using IronMind.Services;
using IronMind.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INutritionRepository, NutritionRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IHydrationRepository, HydrationRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INutritionService, NutritionService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IHydrationService, HydrationService>();

builder.Services.AddHttpClient("OpenFoodFacts", client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "IronMind/1.0 (school project)");
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < 10)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "IronMind API", time = DateTime.UtcNow }))
    .WithTags("Health");

app.MapGet("/db-test", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
            return Results.Problem("Cannot connect to database", statusCode: 503);
        
        return Results.Ok(new
        {
            database = "Connected",
            serverTime = DateTime.UtcNow,
            message = "Database connection successful"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}", statusCode: 503);
    }
})
    .WithTags("Health");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthRoutes();
app.MapNutritionRoutes();
app.MapExerciseRoutes();
app.MapHydrationRoutes();

app.Run();
