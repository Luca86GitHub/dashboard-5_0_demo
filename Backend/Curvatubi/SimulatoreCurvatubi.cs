using Backend.Dashboard;

namespace Backend.Curvatubi;

public class SimulatoreCurvatubi : BackgroundService
{
    private readonly DigitalTwin _twin;
    private readonly ILogger<SimulatoreCurvatubi> _logger;
    private readonly string _memPath; // Cartella condivisa /MEM
    
    // MEMORIA LOCALE: Per non riprocessare sempre lo stesso file
    private HashSet<string> _fileGiaProcessati = new HashSet<string>();

    public SimulatoreCurvatubi(DigitalTwin twin, ILogger<SimulatoreCurvatubi> logger)
    {
        _twin = twin;
        _logger = logger;
        
        // Cartella unica per Input e Output
        _memPath = Path.Combine(Directory.GetCurrentDirectory(), "Dati_Simulati", "SIXTY_MEM");
        Directory.CreateDirectory(_memPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rnd = new Random();
        _logger.LogInformation(">>> [SIXTY] Curvatubi Online. In ascolto su /MEM...");
        
        double tempOlio = 40.0; // Temperatura iniziale olio

        while (!stoppingToken.IsCancellationRequested)
        {
            // CERCA FILE:
            // 1. Deve essere .txt
            // 2. NON deve contenere "REPORT" nel nome (altrimenti leggiamo i nostri stessi output)
            // 3. NON deve essere già stato processato
            var fileOrdine = Directory.GetFiles(_memPath, "*.txt")
                                      .Where(f => !f.Contains("REPORT"))
                                      .FirstOrDefault(f => !_fileGiaProcessati.Contains(f));

            if (fileOrdine != null)
            {
                // --- 1. LETTURA FILE .TXT ---
                string contenuto = await File.ReadAllTextAsync(fileOrdine);
                
                // Valori di default
                string ricetta = "Standard";
                int pezziDaFare = 5;

                try 
                {
                    // Parsing semplice: cerca "Ricetta=..." e "Pezzi=..."
                    var parti = contenuto.Split(';');
                    foreach(var p in parti)
                    {
                        if(p.Trim().StartsWith("Ricetta=")) ricetta = p.Trim().Replace("Ricetta=", "");
                        if(p.Trim().StartsWith("Pezzi=")) pezziDaFare = int.Parse(p.Trim().Replace("Pezzi=", ""));
                    }
                }
                catch { _logger.LogError("Errore formato file Sixty. Uso default."); }

                _logger.LogInformation($"[SIXTY] Inizio Lotto: {ricetta} per {pezziDaFare} pezzi.");

                // --- 2. SETUP MACCHINA ---
                _twin.Sixty.RicettaAttiva = ricetta;
                _twin.Sixty.PezziTarget = pezziDaFare;
                _twin.Sixty.PezziProdotti = 0;
                _twin.Sixty.Stato = "RUN";
                _twin.Sixty.EnergiaTotaleLottoKWh = 0;

                // --- 3. CICLO DI PRODUZIONE (1 Pezzo = 1 Secondo per demo) ---
                for (int i = 0; i < pezziDaFare; i++)
                {
                    // Simuliamo valori fisici
                    _twin.Sixty.SforzoMotore = 85.0 + rnd.NextDouble() * 10; // Sforzo alto piegatura
                    _twin.Sixty.AngoloCurvatura = 45 + rnd.Next(0, 45);      
                    _twin.Sixty.ConsumoIstantaneoKW = 8.0 + rnd.NextDouble(); 
                    
                    // L'olio si scalda lavorando
                    tempOlio += 0.2; 
                    _twin.Sixty.TemperaturaOlio = tempOlio;

                    // Calcolo Energia
                    _twin.Sixty.EnergiaTotaleLottoKWh += _twin.Sixty.ConsumoIstantaneoKW / 3600.0;
                    
                    _twin.Sixty.PezziProdotti++;
                    _twin.Sixty.UltimoAggiornamento = DateTime.Now;

                    // LOG VERBOSO
                    _logger.LogInformation($"[SIXTY RUN] Pezzo {i+1}/{pezziDaFare} | Olio: {tempOlio:F1}°C | Energy: {_twin.Sixty.EnergiaTotaleLottoKWh:F4}");

                    await Task.Delay(1000, stoppingToken);
                }

                // --- 4. FINE LOTTO ---
                _twin.Sixty.Stato = "IDLE";
                _twin.Sixty.ConsumoIstantaneoKW = 0.5;
                _twin.Sixty.SforzoMotore = 0;

                // Generazione File CSV di Output
                string nomeOutput = $"{ricetta}_{DateTime.Now:HHmmss}_REPORT.csv";
                string report = $"Ricetta;Pezzi;Olio_Finale;KWh\n{ricetta};{pezziDaFare};{tempOlio:F1};{_twin.Sixty.EnergiaTotaleLottoKWh:F4}";
                
                await File.WriteAllTextAsync(Path.Combine(_memPath, nomeOutput), report);
                
                _logger.LogInformation($"[SIXTY] Lotto completato. Report creato: {nomeOutput}");
                
                // Segniamo come fatto
                _fileGiaProcessati.Add(fileOrdine);
            }
            else
            {
                // Raffreddamento olio quando ferma
                if (tempOlio > 25) tempOlio -= 0.1;
                _twin.Sixty.TemperaturaOlio = tempOlio;
                
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}