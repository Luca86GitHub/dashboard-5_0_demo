using Backend.Laser;
using Backend.Curvatubi; // <--- Importante: serve per vedere CurvatubiData

namespace Backend.Dashboard;

public class DigitalTwin
{
    // Dati Laser
    public LaserData Laser { get; set; } = new LaserData();

    // Dati Curvatubi (Ecco il pezzo che mancava e causava l'errore!)
    public CurvatubiData Sixty { get; set; } = new CurvatubiData();
}