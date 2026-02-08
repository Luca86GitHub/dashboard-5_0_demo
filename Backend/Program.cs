/*
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
*/

using Backend.Dashboard; 
using Backend.Laser;     
using Backend.Curvatubi; 
using Backend.Gestionale;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAZIONE PORTA ---
builder.WebHost.UseUrls("http://*:5064");

builder.Services.AddSingleton<DigitalTwin>();
builder.Services.AddHostedService<SimulatoreLaser>();
builder.Services.AddHostedService<SimulatoreCurvatubi>();
builder.Services.AddHostedService<SimulatoreGestionale>(); 

builder.Services.AddCors(o => o.AddDefaultPolicy(p => 
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- LOGICA FILE STATICI (CORRETTA E SICURA) ---
var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var browserPath = Path.Combine(webRootPath, "browser");

// Verifichiamo quale cartella esiste davvero per evitare il crash
string? finalStaticPath = null;
if (Directory.Exists(browserPath)) finalStaticPath = browserPath;
else if (Directory.Exists(webRootPath)) finalStaticPath = webRootPath;

// Se abbiamo trovato una cartella valida, configuriamo i file statici
if (finalStaticPath != null)
{
    var fileProvider = new PhysicalFileProvider(finalStaticPath);
    
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
    
    // Fallback solo se siamo in produzione (cartella esistente)
    app.MapFallbackToFile(Directory.Exists(browserPath) ? "browser/index.html" : "index.html");
}

app.UseCors();

// --- API ---
app.MapGet("/api/dashboard", (DigitalTwin data) => data);

app.Run();