using Backend.Dashboard; 
using Backend.Laser;     
using Backend.Curvatubi; 
using Backend.Gestionale;

var builder = WebApplication.CreateBuilder(args);

// 1. Memoria Condivisa
builder.Services.AddSingleton<DigitalTwin>();

// 2. I Simulatori (Laser, Curvatubi, Gestionale)
builder.Services.AddHostedService<SimulatoreLaser>();
builder.Services.AddHostedService<SimulatoreCurvatubi>();
builder.Services.AddHostedService<SimulatoreGestionale>(); 

// 3. Configurazione Web
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors();

// --- API ENDPOINTS (Semplificati) ---

// Nota: ho tolto [FromServices]. .NET capisce da solo cosa iniettare.

app.MapGet("/api/laser", (DigitalTwin data) => data.Laser);

app.MapGet("/api/sixty", (DigitalTwin data) => data.Sixty);

app.MapGet("/api/dashboard", (DigitalTwin data) => data);

app.Run();