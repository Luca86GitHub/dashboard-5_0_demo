using Backend.Dashboard; 
using Backend.Laser;     
using Backend.Curvatubi; 
using Backend.Gestionale;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRAZIONE SERVIZI E SIMULATORI
builder.Services.AddSingleton<DigitalTwin>();
builder.Services.AddHostedService<SimulatoreLaser>();
builder.Services.AddHostedService<SimulatoreCurvatubi>();
builder.Services.AddHostedService<SimulatoreGestionale>(); 

// 2. CONFIGURAZIONE CORS (Utile per lo sviluppo)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => 
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- CONFIGURAZIONE PER SERVIRE IL FRONTEND ANGULAR ---

// Identifichiamo la cartella dove Angular ha creato i file (wwwroot/browser)
var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var browserPath = Path.Combine(webRootPath, "browser");

// Se esiste la cartella "browser", usiamo quella, altrimenti usiamo wwwroot
var finalStaticPath = Directory.Exists(browserPath) ? browserPath : webRootPath;

// Opzioni per servire i file statici
var fileOptions = new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(finalStaticPath),
    RequestPath = "" 
};

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(finalStaticPath)
});

app.UseStaticFiles(fileOptions);

app.UseCors();

// --- API ENDPOINTS ---

app.MapGet("/api/laser", (DigitalTwin data) => data.Laser);
app.MapGet("/api/sixty", (DigitalTwin data) => data.Sixty);
app.MapGet("/api/dashboard", (DigitalTwin data) => data);

// --- GESTIONE ROUTING ANGULAR (FALLBACK) ---
// Se l'utente richiede una rotta che non è un'API, serviamo index.html
app.MapFallbackToFile(Directory.Exists(browserPath) ? "browser/index.html" : "index.html");

app.Run();