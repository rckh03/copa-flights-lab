var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

// 1. API básica + log estructurado
var flights = new[]
{
    new Flight("CM101", "PTY", "MIA", "08:30"),
    new Flight("CM245", "PTY", "BOG", "11:15"),
    new Flight("CM530", "PTY", "MEX", "14:40"),
};

app.MapGet("/api/flights", (ILogger<Program> log) =>
{
    log.LogInformation("Listando {Count} vuelos", flights.Length);
    return Results.Ok(flights);
});

// 2. Health check
app.MapHealthChecks("/health");

// 3. Excepción intencional -> 500
app.MapGet("/api/boom", (ILogger<Program> log) =>
{
    log.LogWarning("Alguien llamó a /api/boom");
    throw new InvalidOperationException("Fallo simulado para practicar diagnóstico");
});

// 4. Variable de configuración obligatoria
app.MapGet("/api/config-check", (IConfiguration cfg, ILogger<Program> log) =>
{
    var key = cfg["FLIGHTS_API_KEY"];
    if (string.IsNullOrEmpty(key))
    {
        log.LogError("Falta la variable FLIGHTS_API_KEY");
        throw new InvalidOperationException("FLIGHTS_API_KEY no está configurada");
    }
    return Results.Ok(new { configured = true });
});

// 5. Endpoint lento (para métricas/trazas más adelante)
app.MapGet("/api/slow", async (ILogger<Program> log) =>
{
    var ms = Random.Shared.Next(500, 3000);
    await Task.Delay(ms);
    log.LogInformation("Respuesta lenta de {Ms} ms", ms);
    return Results.Ok(new { delayMs = ms });
});

app.Run();

record Flight(string Code, string From, string To, string Departure);