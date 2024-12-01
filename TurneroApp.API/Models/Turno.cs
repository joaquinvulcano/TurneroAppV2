namespace TurneroApp.API.Models
{
    public class Turno
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string Nombre { get; set; }
        public string TipoServicio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; }
    }
}
