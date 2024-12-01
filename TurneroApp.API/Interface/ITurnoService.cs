using TurneroApp.API.DTOs;

namespace TurneroApp.API.Interface
{
    public interface ITurnoService
    {
        Task<TurnoDto> CrearTurnoAsync(TurnoCreateDto turnoCreateDto);
        Task<TurnoDto> LlamarSiguienteTurnoAsync();
        Task<HistorialTurnoDto> RegistrarHistorialTurnoAsync(string numeroTurno, string estado);
        Task<bool> CancelarTurnoAsync(string numeroTurno);
        Task<List<TurnoDto>> ObtenerProximosTurnosAsync();
        Task<bool> CancelarLlamadaAsync(string numeroTurno);
        Task<int> ContarTurnosPendientesAsync();
        Task<bool> AgregarServicioAsync(string nombreServicio);
        Task<List<ServicioDto>> ObtenerServiciosAsync();
        Task<bool> EliminarServicioAsync(string nombreServicio);
        Task<bool> ResetearTurnosAsync();
        Task<object> ObtenerEstadisticasAsync();
    }
}
