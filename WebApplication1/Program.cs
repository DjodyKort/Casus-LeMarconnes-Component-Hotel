// ======== Imports ========
using Microsoft.EntityFrameworkCore;
using LeMarconnes.API.DAL;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.DAL.Repositories;

// ============================================================
// BUILDER CONFIGURATION
// Entry point van de ASP.NET Core Web API. Services en middleware.
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ==== Add services to the container ====

// Controllers
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==== Database Configuration (EF Core) ====
// We halen de connection string op en koppelen de Context aan MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LeMarconnesContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// ==== Repository Injection ====
// Koppel de Interface aan de Implementatie
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

// ============================================================
// APP CONFIGURATION
// Configureer de HTTP pipeline
// ============================================================

var app = builder.Build();

// ==== Database Seeding (Code First) ====
// Dit blok voert migraties uit en vult de database met startdata (Hotelkamers)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LeMarconnesContext>();
        // Roep de Seeder aan die we eerder hebben gemaakt
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Er is een fout opgetreden bij het seeden van de database.");
    }
}

// ==== Middleware ====
if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================================
// START APPLICATION
// ============================================================

app.Run();