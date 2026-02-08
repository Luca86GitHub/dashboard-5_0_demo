namespace Backend.Curvatubi;

public class CurvatubiData
{
    // Stato Macchina
    public string Stato { get; set; } = "IDLE"; 
    public string RicettaAttiva { get; set; } = "N/A";
    
    // Contatori Produzione
    public int PezziProdotti { get; set; }
    public int PezziTarget { get; set; }

    // Telemetria (Dati fisici specifici della piegatrice)
    public double AngoloCurvatura { get; set; }       // Gradi
    public double TemperaturaOlio { get; set; }       // °C
    public double SforzoMotore { get; set; }          // %
    
    // Energia
    public double ConsumoIstantaneoKW { get; set; }
    public double EnergiaTotaleLottoKWh { get; set; } 
    
    public DateTime UltimoAggiornamento { get; set; }
}