using Microsoft.EntityFrameworkCore;
using TurneroApp.API.Models;

namespace TurneroApp.API.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<HistorialTurno> HistorialTurnos { get; set; }
    }

}
