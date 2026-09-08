var builder = WebApplication.CreateBuilder(args);

// Telemetria, health checks, service discovery i resilience — wspólne dla wszystkich usług.
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// /health i /alive — konsumowane przez dashboard Aspire (tylko w Development).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
