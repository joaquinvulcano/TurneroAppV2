using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TurneroApp.API.Context;
using TurneroApp.API.DTOs;
using TurneroApp.API.Interface;
using TurneroApp.API.Models;

namespace TurneroApp.API.Service
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio relacionada con los turnos.
    /// </summary>
    public class TurnoService : ITurnoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TurnoHub> _hubContext;
        private readonly LoggerService _logger;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TurnoService"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos.</param>
        /// <param name="hubContext">El contexto del hub de SignalR.</param>
        public TurnoService(ApplicationDbContext context, IHubContext<TurnoHub> hubContext, LoggerService logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        #region Metodos Publicos

        /// <summary>
        /// Crea un nuevo turno de manera asíncrona.
        /// </summary>
        /// <param name="turnoCreateDto">Los datos del turno a crear.</param>
        /// <returns>Un objeto <see cref="TurnoDto"/> que representa el turno creado.</returns>
        public async Task<TurnoDto> CrearTurnoAsync(TurnoCreateDto turnoCreateDto)
        {
            try
            {
                var ultimoTurno = await _context.Turnos
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefaultAsync();

                // Verificar si ultimoTurno es null y manejarlo
                var nuevoNumero = GenerarSiguienteNumeroTurno(ultimoTurno?.Numero);

                var turno = new Turno
                {
                    Numero = nuevoNumero,
                    Nombre = turnoCreateDto.Nombre,
                    TipoServicio = turnoCreateDto.TipoServicio,
                    FechaCreacion = DateTime.Now,
                    Estado = "Pendiente"
                };

                // Incrementar la cantidad de solicitudes del servicio
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.Nombre == turnoCreateDto.TipoServicio);

                if (servicio != null)
                {
                    servicio.CantidadSolicitudes++;
                    _context.Servicios.Update(servicio);
                }
                else
                {
                    // Manejar el caso donde el servicio no existe
                    throw new Exception($"El servicio '{turnoCreateDto.TipoServicio}' no existe.");
                }

                _context.Turnos.Add(turno);
                await _context.SaveChangesAsync();

                return new TurnoDto { Numero = turno.Numero, Nombre = turno.Nombre, TipoServicio = turno.TipoServicio, Estado = turno.Estado };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el turno: {ex.Message}");
                throw new Exception("Error al crear el turno.", ex);
            }
        }

        /// <summary>
        /// Cancela un turno de manera asíncrona.
        /// </summary>
        /// <param name="numeroTurno">El número del turno a cancelar.</param>
        /// <returns>True si el turno fue cancelado, de lo contrario, false.</returns>
        public async Task<bool> CancelarTurnoAsync(string numeroTurno)
        {
            try
            {
                var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Numero == numeroTurno);
                if (turno == null)
                    return false;

                turno.Estado = "Cancelado";
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cancelar el turno: {ex.Message}");
                throw new Exception("Error al cancelar el turno.", ex);
            }
        }

        public async Task<HistorialTurnoDto> RegistrarHistorialTurnoAsync(string numeroTurno, string estado)
        {
            try
            {
                var historial = new HistorialTurno
                {
                    TurnoNumero = numeroTurno,
                    FechaHora = DateTime.Now,
                    Estado = estado
                };

                _context.HistorialTurnos.Add(historial);
                await _context.SaveChangesAsync();

                return new HistorialTurnoDto { TurnoNumero = historial.TurnoNumero, FechaHora = historial.FechaHora, Estado = historial.Estado };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al registrar el historial del turno: {ex.Message}");
                throw new Exception("Error al registrar el historial del turno.", ex);
            }
        }

        public async Task<TurnoDto> LlamarSiguienteTurnoAsync()
        {
            try
            {
                var siguienteTurno = await _context.Turnos
                    .Where(t => t.Estado == "Pendiente")
                    .OrderBy(t => t.FechaCreacion)
                    .FirstOrDefaultAsync();

                if (siguienteTurno == null)
                {
                    throw new InvalidOperationException("No hay turnos pendientes.");
                }

                siguienteTurno.Estado = "Llamado";
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("RecibirTurno", siguienteTurno);

                // Obtener y enviar la lista actualizada de próximos turnos
                var proximosTurnos = await ObtenerProximosTurnosAsync();
                await _hubContext.Clients.All.SendAsync("ActualizarProximosTurnos", proximosTurnos);

                return new TurnoDto
                {
                    Numero = siguienteTurno.Numero,
                    Estado = siguienteTurno.Estado,
                    Nombre = siguienteTurno.Nombre,
                    TipoServicio = siguienteTurno.TipoServicio
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al llamar al siguiente turno: {ex.Message}");
                throw new Exception("Error al llamar al siguiente turno.", ex);
            }
        }

        public async Task<List<TurnoDto>> ObtenerProximosTurnosAsync()
        {
            try
            {
                var proximosTurnos = await _context.Turnos
                    .Where(t => t.Estado == "Pendiente")
                    .OrderBy(t => t.FechaCreacion)
                    .Take(6)
                    .ToListAsync();

                // Registro para depuración
                Console.WriteLine($"Turnos obtenidos: {string.Join(", ", proximosTurnos.Select(t => t.Numero))}");

                return proximosTurnos.Select(t => new TurnoDto
                {
                    Numero = t.Numero,
                    Nombre = t.Nombre,
                    TipoServicio = t.TipoServicio
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los próximos turnos: {ex.Message}");
                throw new Exception("Error al obtener los próximos turnos.", ex);
            }
        }

        /// <summary>
        /// Cancela la llamada de un turno de manera asíncrona.
        /// </summary>
        /// <param name="numeroTurno">El número del turno a cancelar.</param>
        /// <returns>True si el turno fue cancelado, de lo contrario, false.</returns>
        public async Task<bool> CancelarLlamadaAsync(string numeroTurno)
        {
            try
            {
                var turnoActual = await _context.Turnos.FirstOrDefaultAsync(t => t.Numero == numeroTurno);
                if (turnoActual == null)
                    return false;

                turnoActual.Estado = "Pendiente";
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ActualizarTurno", turnoActual);

                var proximosTurnos = await ObtenerProximosTurnosAsync();
                await _hubContext.Clients.All.SendAsync("ActualizarProximosTurnos", proximosTurnos);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cancelar la llamada del turno: {ex.Message}");
                throw new Exception("Error al cancelar la llamada del turno.", ex);
            }
        }
        public async Task<int> ContarTurnosPendientesAsync()
        {
            try
            {
                return await _context.Turnos.CountAsync(t => t.Estado == "Pendiente");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al contar los turnos pendientes: {ex.Message}");
                throw new Exception("Error al contar los turnos pendientes.", ex);
            }
        }

        public async Task<bool> AgregarServicioAsync(string nombreServicio)
        {
            try
            {
                // Verificar si el servicio ya existe
                var servicioExistente = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.Nombre == nombreServicio);

                if (servicioExistente != null)
                {
                    return false; // El servicio ya existe
                }

                // Crear un nuevo servicio
                var nuevoServicio = new Servicio
                {
                    Nombre = nombreServicio
                };

                // Agregar el nuevo servicio al contexto
                _context.Servicios.Add(nuevoServicio);
                await _context.SaveChangesAsync();

                return true; // Servicio agregado exitosamente
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al agregar el servicio: {ex.Message}");
                throw new Exception("Error al agregar el servicio.", ex);
            }
        }

        public async Task<List<ServicioDto>> ObtenerServiciosAsync()
        {
            try
            {
                var servicios = await _context.Servicios.ToListAsync();

                // Convertir a DTO
                return servicios.Select(s => new ServicioDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener la lista de servicios: {ex.Message}");
                throw new Exception("Error al obtener la lista de servicios.", ex);
            }
        }

        public async Task<bool> EliminarServicioAsync(string nombreServicio)
        {
            try
            {
                // Buscar el servicio en la base de datos
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.Nombre == nombreServicio);

                if (servicio == null)
                {
                    return false; // El servicio no existe
                }

                // Eliminar el servicio del contexto
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();

                return true; // Servicio eliminado exitosamente
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el servicio: {ex.Message}");
                throw new Exception("Error al eliminar el servicio.", ex);
            }
        }

        public async Task<bool> ResetearTurnosAsync()
        {
            try
            {
                // Eliminar todos los turnos existentes
                var turnosExistentes = await _context.Turnos.ToListAsync();
                if (turnosExistentes.Any())
                {
                    _context.Turnos.RemoveRange(turnosExistentes);
                    await _context.SaveChangesAsync();
                }

                return true; // Turnos eliminados exitosamente
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al resetear los turnos: {ex.Message}");
                throw new Exception("Error al resetear los turnos.", ex);
            }
        }


        public async Task<object> ObtenerEstadisticasAsync()
        {
            try
            {
                var turnosAtendidos = await ContarTurnosAtendidosAsync();
                var turnosEnEspera = await ContarTurnosEnEsperaAsync();
                var serviciosSolicitados = await ObtenerServiciosMasSolicitadosAsync();

                return new
                {
                    turnosAtendidos,
                    turnosEnEspera,
                    serviciosSolicitados
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener las estadísticas: {ex.Message}");
                throw new Exception("Error al obtener las estadísticas.", ex);
            }
        }

        public async Task<int> ContarTurnosAtendidosAsync()
        {
            return await _context.Turnos.CountAsync(t => t.Estado == "Atendido");
        }

        public async Task<int> ContarTurnosEnEsperaAsync()
        {
            return await _context.Turnos.CountAsync(t => t.Estado == "Pendiente");
        }

        public async Task<List<ServicioDto>> ObtenerServiciosMasSolicitadosAsync()
        {
            return await _context.Servicios
                .Select(s => new ServicioDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Value = s.CantidadSolicitudes // Usar el nuevo campo
                })
                .ToListAsync();
        }

        #endregion

        #region Metodos Privados
        /// <summary>
        /// Genera el siguiente número de turno basado en el último número.
        /// </summary>
        /// <param name="ultimoNumero">El último número de turno.</param>
        /// <returns>El siguiente número de turno.</returns>
        private string GenerarSiguienteNumeroTurno(string ultimoNumero)
        {
            try
            {
                if (string.IsNullOrEmpty(ultimoNumero))
                {
                    return "A001";
                }

                if (ultimoNumero.Length != 4 || !char.IsLetter(ultimoNumero[0]) || !int.TryParse(ultimoNumero.Substring(1), out int numero))
                {
                    throw new FormatException("El número de turno tiene un formato incorrecto.");
                }

                var letra = ultimoNumero[0];

                if (numero >= 999)
                {
                    letra = (char)(letra + 1);
                    numero = 1;
                }
                else
                {
                    numero++;
                }

                return $"{letra}{numero:D3}";
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                return "A001";
            }
        }
        #endregion
    }
}
