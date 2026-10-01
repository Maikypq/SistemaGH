using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;

namespace SistemaGH.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Empleado> Empleados { get; set; }
    }
}