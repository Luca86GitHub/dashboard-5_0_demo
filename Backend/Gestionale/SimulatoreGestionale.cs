namespace Backend.Gestionale;

public class SimulatoreGestionale : BackgroundService
{
    private readonly ILogger<SimulatoreGestionale> _logger;
    private readonly string _inboxLaser;
    private readonly string _memSixty;

    public SimulatoreGestionale(ILogger<SimulatoreGestionale> logger)
    {
        _logger = logger;
        // Puntiamo alle stesse cartelle che ascoltano le macchine
        _inboxLaser = Path.Combine(Directory.GetCurrentDirectory(), "Dati_Simulati", "LT5_INBOX");
        _memSixty = Path.Combine(Directory.GetCurrentDirectory(), "Dati_Simulati", "SIXTY_MEM");
        
        // Creiamo (per sicurezza, anche se ci sono già)
        Directory.CreateDirectory(_inboxLaser);
        Directory.CreateDirectory(_memSixty);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rnd = new Random();
        _logger.LogInformation(">>> [GESTIONALE] ERP System Online. Inizio invio ordini automatici...");

        // Aspetta 5 secondi all'avvio per dare tempo alle macchine di accendersi
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Decidiamo a chi mandare l'ordine (o entrambi, o uno solo)
            int scelta = rnd.Next(0, 10); // Numero da 0 a 9

            if (scelta < 5) // 50% probabilità: Ordine LASER
            {
                await GeneraOrdineLaser(rnd);
            }
            else // 50% probabilità: Ordine CURVATUBI
            {
                await GeneraOrdineCurvatubi(rnd);
            }

            // Pausa casuale tra un invio e l'altro (es. tra 15 e 30 secondi)
            int attesa = rnd.Next(15000, 30000);
            _logger.LogInformation($"[GESTIONALE] Prossimo ordine tra {attesa/1000} secondi...");
            await Task.Delay(attesa, stoppingToken);
        }
    }

    private async Task GeneraOrdineLaser(Random rnd)
    {
        string id = $"ORD-L-{rnd.Next(1000, 9999)}";
        string materiale = (rnd.Next(0, 2) == 0) ? "Ferro" : "Acciaio";
        int spessore = rnd.Next(2, 15); // Da 2 a 15mm
        int pezzi = rnd.Next(5, 15);    // Durata in secondi

        string xmlContent = $@"
<Job>
  <ID>{id}</ID>
  <Materiale>{materiale}</Materiale>
  <Spessore>{spessore}</Spessore>
  <Quantita>{pezzi}</Quantita>
</Job>";

        string path = Path.Combine(_inboxLaser, $"{id}.xml");
        await File.WriteAllTextAsync(path, xmlContent);
        
        _logger.LogInformation($"[GESTIONALE] >>> Inviato ordine LASER: {id} ({materiale} {spessore}mm)");
    }

    private async Task GeneraOrdineCurvatubi(Random rnd)
    {
        string id = $"ORD-C-{rnd.Next(1000, 9999)}";
        string ricetta = (rnd.Next(0, 2) == 0) ? "Marmitta_Sport" : "Telaio_Moto";
        int pezzi = rnd.Next(5, 10);

        string txtContent = $"Ricetta={ricetta};Pezzi={pezzi}";

        string path = Path.Combine(_memSixty, $"{id}.txt");
        await File.WriteAllTextAsync(path, txtContent);

        _logger.LogInformation($"[GESTIONALE] >>> Inviato ordine SIXTY: {id} ({ricetta})");
    }
}