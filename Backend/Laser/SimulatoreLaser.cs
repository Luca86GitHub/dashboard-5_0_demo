using Backend.Dashboard;
using System.Xml.Linq; 

namespace Backend.Laser;

public class SimulatoreLaser : BackgroundService
{
    private readonly DigitalTwin _twin;
    private readonly ILogger<SimulatoreLaser> _logger;
    private readonly string _inboxPath;
    private readonly string _outboxPath;

    // MEMORIA LOCALE: Qui ci segniamo i file fatti per non rifarli subito
    private HashSet<string> _fileGiaProcessati = new HashSet<string>();

    public SimulatoreLaser(DigitalTwin twin, ILogger<SimulatoreLaser> logger)
    {
        _twin = twin;
        _logger = logger;
        _inboxPath = Path.Combine(Directory.GetCurrentDirectory(), "Dati_Simulati", "LT5_INBOX");
        _outboxPath = Path.Combine(Directory.GetCurrentDirectory(), "Dati_Simulati", "LT5_OUTBOX");
        
        Directory.CreateDirectory(_inboxPath);
        Directory.CreateDirectory(_outboxPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rnd = new Random();
        _logger.LogInformation(">>> [LASER] Modulo Avviato. In attesa in LT5_INBOX...");

        while (!stoppingToken.IsCancellationRequested)
        {
            // CERCA FILE: Prendi i file .xml MA SOLO quelli che NON sono nella lista dei processati
            var fileOrdine = Directory.GetFiles(_inboxPath, "*.xml")
                                      .FirstOrDefault(f => !_fileGiaProcessati.Contains(f));

            if (fileOrdine != null)
            {
                try 
                {
                    // --- 1. LETTURA ---
                    XDocument doc = XDocument.Load(fileOrdine);
                    
                    string idCommessa = doc.Root?.Element("ID")?.Value ?? "SCONOSCIUTO";
                    string materiale = doc.Root?.Element("Materiale")?.Value ?? "Ferro";
                    string strSpessore = doc.Root?.Element("Spessore")?.Value ?? "1";
                    double spessore = double.Parse(strSpessore);
                    string strQuantita = doc.Root?.Element("Quantita")?.Value ?? "10";
                    int quantita = int.Parse(strQuantita);

                    // --- 2. SETUP ---
                    _twin.Laser.CommessaAttuale = idCommessa;
                    _twin.Laser.Stato = "RUN";
                    _twin.Laser.EnergiaTotaleJobKWh = 0;
                    
                    // --- 3. PARAMETRI ---
                    double targetPower = 2000 + (spessore * 100); 
                    double targetSpeed = 3000 - (spessore * 100); 
                    if (targetSpeed < 500) targetSpeed = 500;
                    int durataLavoro = quantita; 

                    _logger.LogInformation($"[LASER] Inizio Lavoro: {idCommessa} ({quantita} secondi)");

                    // --- 4. ESECUZIONE (LOOP) ---
                    for (int i = 0; i < durataLavoro; i++)
                    {
                        _twin.Laser.PotenzaWatt = targetPower + rnd.Next(-50, 50);
                        _twin.Laser.VelocitaTaglio = targetSpeed + rnd.Next(-20, 20);
                        _twin.Laser.PressioneGas = (materiale == "Acciaio") ? 15.0 : 5.0; 
                        
                        _twin.Laser.ConsumoIstantaneoKW = 10.0 + (_twin.Laser.PotenzaWatt / 1000.0 * 3.0);
                        _twin.Laser.EnergiaTotaleJobKWh += _twin.Laser.ConsumoIstantaneoKW / 3600.0;
                        _twin.Laser.UltimoAggiornamento = DateTime.Now;

                        _logger.LogInformation($"[RUN] {i+1}/{durataLavoro} sec | Pwr: {_twin.Laser.PotenzaWatt:F0}W | Consumo: {_twin.Laser.ConsumoIstantaneoKW:F1}kW");

                        await Task.Delay(1000, stoppingToken);
                    }

                    // --- 5. FINE ---
                    _twin.Laser.Stato = "IDLE";
                    _twin.Laser.PotenzaWatt = 0;
                    _twin.Laser.ConsumoIstantaneoKW = 0.5;

                    // Scrittura Report
                    string report = $"ID;Materiale;Spessore;Pezzi;KWh_Totali\n{idCommessa};{materiale};{spessore};{quantita};{_twin.Laser.EnergiaTotaleJobKWh:F4}";
                    await File.WriteAllTextAsync(Path.Combine(_outboxPath, $"{idCommessa}_REPORT.csv"), report);
                    
                    _logger.LogInformation($"[LASER] Completato. In attesa di NUOVI file...");
                    
                    // *** PUNTO CHIAVE: Segniamo questo file come "Fatto" ***
                    _fileGiaProcessati.Add(fileOrdine);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERRORE: {ex.Message}");
                    // Se va in errore, lo ignoriamo comunque per non bloccarci
                    _fileGiaProcessati.Add(fileOrdine);
                }
            }
            else
            {
                // Nessun NUOVO file trovato
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}