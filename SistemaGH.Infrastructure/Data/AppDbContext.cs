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
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<PerfilPermiso> PerfilPermisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la llave primaria compuesta para la tabla intermedia
            modelBuilder.Entity<PerfilPermiso>()
                .HasKey(pp => new { pp.PerfilId, pp.PermisoId });
        }
    }
}