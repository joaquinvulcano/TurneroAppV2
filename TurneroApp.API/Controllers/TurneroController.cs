using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurneroApp.API.DTOs;
using TurneroApp.API.Interface;

namespace TurneroApp.API.Controllers
{
    [Route("api/Turno")]
    [ApiController]
    public class TurneroController : ControllerBase
    {
        private readonly ITurnoService _turnoService;

        public TurneroController(ITurnoService turnoService)
        {
            _turnoService = turnoService;
        }

        /// <summary>
        /// Crea un nuevo turno de manera asíncrona.
        /// </summary>
        /// <param name="turnoCreateDto">Los datos del turno a crear.</param>
        /// <returns>Un objeto <see cref="TurnoDto"/> que representa el turno creado.</returns>
        [HttpPost]
        [Authorize]

        public async Task<ActionResult<TurnoDto>> CrearTurno(TurnoCreateDto turnoCreateDto)
        {
            try
            {
                var turno = await _turnoService.CrearTurnoAsync(turnoCreateDto);
                return Ok(turno);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al crear el turno.");
            }
        }

        /// <summary>
        /// Llama al siguiente turno de manera asíncrona.
        /// </summary>
        /// <returns>Un objeto <see cref="TurnoDto"/> que representa el siguiente turno.</returns>
        [HttpPut("llamar-siguiente")]
        [Authorize]

        public async Task<ActionResult<TurnoDto>> LlamarSiguienteTurno()
        {
            try
            {
                var turno = await _turnoService.LlamarSiguienteTurnoAsync();
                return Ok(turno);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al llamar al siguiente turno.");
            }
        }

        /// <summary>
        /// Cancela un turno de manera asíncrona.
        /// </summary>
        /// <param name="numeroTurno">El número del turno a cancelar.</param>
        /// <returns>True si el turno fue cancelado, de lo contrario, false.</returns>
        [HttpPut("cancelar/{numeroTurno}")]
        [Authorize]

        public async Task<ActionResult<bool>> CancelarTurno(string numeroTurno)
        {
            try
            {
                var resultado = await _turnoService.CancelarTurnoAsync(numeroTurno);
                if (!resultado)
                {
                    return NotFound(new { mensaje = "Turno no encontrado." });
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al cancelar el turno.");
            }
        }

        /// <summary>
        /// Cancela una llamada de turno de manera asíncrona.
        /// </summary>
        /// <param name="numero">El número del turno cuya llamada se desea cancelar.</param>
        /// <returns>Un mensaje de éxito o un error si el turno no se encuentra.</returns>
        [HttpPut("cancelar-llamada/{numero}")]
        [Authorize]

        public async Task<IActionResult> CancelarLlamada(string numero)
        {
            try
            {
                var result = await _turnoService.CancelarLlamadaAsync(numero);
                if (!result)
                {
                    return NotFound(new { mensaje = "Turno no encontrado" });
                }

                return Ok(new { mensaje = "La llamada del turno ha sido cancelada y el turno está nuevamente pendiente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al cancelar la llamada del turno.");
            }
        }

        /// <summary>
        /// Obtiene los próximos turnos de manera asíncrona.
        /// </summary>
        /// <returns>Una lista de próximos turnos.</returns>
        [HttpGet("proximos-turnos")]
        [Authorize]

        public async Task<IActionResult> GetProximosTurnos()
        {
            try
            {
                var proximosTurnos = await _turnoService.ObtenerProximosTurnosAsync();
                return Ok(proximosTurnos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al obtener los próximos turnos.");
            }
        }

        /// <summary>
        /// Obtiene la cantidad de turnos pendientes de manera asíncrona.
        /// </summary>
        /// <returns>Una cuenta de la cantidad de turnos pendientes.</returns>
        [HttpGet("turnos-pendientes")]
        [Authorize]

        public async Task<IActionResult> GetTurnosPendientes()
        {
            try
            {
                var turnosPendientes = await _turnoService.ContarTurnosPendientesAsync();
                return Ok(turnosPendientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al obtener la cantidad de turnos pendientes.");
            }
        }

        /// <summary>
        /// Agrega un servicio a un turno existente de manera asíncrona.
        /// </summary>
        /// <param name="nombreServicio">El nombre del servicio a agregar.</param>
        /// <returns>Un mensaje de éxito o un error si el servicio ya existe.</returns>
        [HttpPost("agregar-servicio")]
        [Authorize]

        public async Task<IActionResult> AgregarServicio([FromBody] string nombreServicio)
        {
            try
            {
                var result = await _turnoService.AgregarServicioAsync(nombreServicio);
                if (!result)
                {
                    return BadRequest(new { mensaje = "El servicio ya existe." });
                }

                return Ok(new { mensaje = "Servicio agregado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al agregar el servicio.");
            }
        }

        /// <summary>
        /// Obtiene la lista de servicios de manera asíncrona.
        /// </summary>
        /// <returns>Una lista de servicios.</returns>
        [HttpGet("traer-servicios")]
        [Authorize]

        public async Task<IActionResult> GetServicios()
        {
            try
            {
                var servicios = await _turnoService.ObtenerServiciosAsync();
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al obtener la lista de servicios.");
            }
        }

        /// <summary>
        /// Elimina un servicio de manera asíncrona.
        /// </summary>
        /// <param name="nombreServicio">El nombre del servicio a eliminar.</param>
        /// <returns>Un mensaje de éxito o un error si el servicio no existe.</returns>
        [HttpDelete("eliminar-servicio/{nombreServicio}")]
        [Authorize]

        public async Task<IActionResult> EliminarServicio(string nombreServicio)
        {
            try
            {
                var result = await _turnoService.EliminarServicioAsync(nombreServicio);
                if (!result)
                {
                    return NotFound(new { mensaje = "El servicio no existe." });
                }

                return Ok(new { mensaje = "Servicio eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al eliminar el servicio.");
            }
        }

        /// <summary>
        /// Resetea todos los turnos de manera asíncrona.
        /// </summary>
        /// <returns>Un mensaje de éxito o un error si ocurre un problema.</returns>
        [HttpDelete("resetear-turnos")]
        [Authorize]

        public async Task<IActionResult> ResetearTurnos()
        {
            try
            {
                var result = await _turnoService.ResetearTurnosAsync();
                if (!result)
                {
                    return BadRequest(new { mensaje = "Error al resetear los turnos." });
                }

                return Ok(new { mensaje = "Todos los turnos han sido eliminados exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al resetear los turnos.");
            }
        }

        /// <summary>
        /// Obtiene estadísticas sobre los turnos de manera asíncrona.
        /// </summary>
        /// <returns>Un objeto con las estadísticas de los turnos.</returns>
        [HttpGet("estadisticas")]
        [Authorize]

        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                var estadisticas = await _turnoService.ObtenerEstadisticasAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al obtener las estadísticas de los turnos.");
            }
        }
    }
}
