namespace TurneroApp.API.Models
{
    public class HistorialTurno
    {
        public int Id { get; set; }
        public string TurnoNumero { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
    }

}