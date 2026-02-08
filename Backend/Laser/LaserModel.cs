namespace Backend.Laser;

public class LaserData
{
    // Stato OPC-UA
    public string Stato { get; set; } = "IDLE"; 
    public string CommessaAttuale { get; set; } = "N/A";
    
    // Telemetria
    public double PotenzaWatt { get; set; }
    public double PressioneGas { get; set; }
    public double VelocitaTaglio { get; set; }
    
    // Energia
    public double ConsumoIstantaneoKW { get; set; }
    public double EnergiaTotaleJobKWh { get; set; } 
    
    // Timestamp
    public DateTime UltimoAggiornamento { get; set; }
}