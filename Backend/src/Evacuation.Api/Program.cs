using Evacuation.Application.Imaging;
using Evacuation.Infrastructure.Imaging;
using Evacuation.Infrastructure.Persistence;
using Evacuation.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Telemetria, health checks, service discovery i resilience — wspólne dla wszystkich usług.
builder.AddServiceDefaults();

// Nazwa "evacuationdb" odpowiada bazie zadeklarowanej w AppHost.cs. Aspire wstrzykuje pod nią
// connection string, dokłada health check bazy i podpina ją do telemetrii.
builder.AddNpgsqlDbContext<EvacuationDbContext>(connectionName: "evacuationdb");

// Pipeline obrazu i dostęp do grafu. Dekoder jest bezstanowy, pozostałe dwa trzymają DbContext,
// więc żyją tyle co zapytanie.
builder.Services.AddSingleton<IPlanImageDecoder, PngPlanImageDecoder>();
builder.Services.AddScoped<FloorPlanImporter>();
builder.Services.AddScoped<FloorGraphLoader>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// /health i /alive — konsumowane przez dashboard Aspire (tylko w Development).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await ApplyMigrationsAsync(app);
}

// Poza Development przekierowujemy na HTTPS. W trakcie pracy nad aplikacją mobilną nie —
// telefon nie ufa deweloperskiemu certyfikatowi komputera, więc przekierowanie zerwałoby
// mu połączenie, zanim zdążyłby cokolwiek pobrać.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

/// <summary>
/// Dociąga schemat bazy przy starcie.
/// </summary>
/// <remarks>
/// Wygodne w środowisku deweloperskim: AppHost podnosi pusty kontener Postgresa, a API samo
/// zakłada tabele. Celowo tylko w Development — przy wielu instancjach aplikacji każda próbowałaby
/// migrować równolegle, a wdrożenie produkcyjne powinno stosować migracje osobnym krokiem,
/// przed uruchomieniem usługi.
/// </remarks>
static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<EvacuationDbContext>();

    await context.Database.MigrateAsync();
}
