using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;

namespace SistemaGH.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }
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

            // ==========================================
            // 1. SEEDING DE PERMISOS BASE (Agrupados por Módulos)
            // ==========================================
            modelBuilder.Entity<Permiso>().HasData(
                // Módulo Usuarios
                new Permiso { Id = 1, Modulo = "Usuarios", Accion = "Ver Listado", CodigoSistema = "Usuarios.Ver" },
                new Permiso { Id = 2, Modulo = "Usuarios", Accion = "Crear Usuario", CodigoSistema = "Usuarios.Crear" },
                new Permiso { Id = 3, Modulo = "Usuarios", Accion = "Editar Usuario", CodigoSistema = "Usuarios.Editar" },
                new Permiso { Id = 4, Modulo = "Usuarios", Accion = "Eliminar Usuario", CodigoSistema = "Usuarios.Eliminar" },

                // Módulo Perfiles
                new Permiso { Id = 5, Modulo = "Perfiles", Accion = "Ver Perfiles", CodigoSistema = "Perfiles.Ver" },
                new Permiso { Id = 6, Modulo = "Perfiles", Accion = "Gestionar Permisos", CodigoSistema = "Perfiles.GestionarPermisos" },

                // Módulo Empleados
                new Permiso { Id = 7, Modulo = "Empleados", Accion = "Ver Empleados", CodigoSistema = "Empleados.Ver" },
                new Permiso { Id = 8, Modulo = "Empleados", Accion = "Crear Empleado", CodigoSistema = "Empleados.Crear" }
            );

            // ==========================================
            // 2. SEEDING DE PERFILES
            // ==========================================
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { Id = 1, Nombre = "Administrador", Descripcion = "Perfil con acceso total al sistema", Activo = true },
                new Perfil { Id = 2, Nombre = "Operador", Descripcion = "Perfil operativo estándar", Activo = true }
            );

            // ==========================================
            // 3. SEEDING DE RELACIÓN PERFIL-PERMISOS (Admin tiene todos: del 1 al 8)
            // ==========================================
            var perfilPermisosSeed = new List<PerfilPermiso>();
            for (int i = 1; i <= 8; i++)
            {
                perfilPermisosSeed.Add(new PerfilPermiso { PerfilId = 1, PermisoId = i });
            }
            modelBuilder.Entity<PerfilPermiso>().HasData(perfilPermisosSeed);

        }
    }
}