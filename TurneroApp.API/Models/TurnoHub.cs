using Microsoft.AspNetCore.SignalR;
using TurneroApp.API.DTOs;

namespace TurneroApp.API.Models
{
    public class TurnoHub : Hub
    {
        public async Task EnviarTurno(TurnoDto turno)
        {
            await Clients.All.SendAsync("RecibirTurno", turno);
        }

        public async Task ActualizarTurno(TurnoDto turno)
        {
            await Clients.All.SendAsync("ActualizarTurno", turno);
        }

        public async Task ActualizarProximosTurnos(List<TurnoDto> proximosTurnos)
        {
            await Clients.All.SendAsync("ActualizarProximosTurnos", proximosTurnos);
        }
    }
}
