using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Data;

namespace SistemaGH.Infrastructure.Repositories
{
    public class PerfilRepository : IPerfilRepository
    {
        private readonly AppDbContext _context;

        public PerfilRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Perfil>> ObtenerTodosAsync()
        {
            return await _context.Perfiles.ToListAsync();
        }

        public async Task<Perfil?> ObtenerPorIdConPermisosAsync(int id)
        {
            return await _context.Perfiles
                .Include(p => p.PerfilPermisos)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Permiso>> ObtenerTodosLosPermisosAsync()
        {
            return await _context.Permisos.ToListAsync();
        }

        public async Task ActualizarPermisosAsync(int perfilId, List<int> permisosSeleccionados, string usuarioAuditor)
        {
            var perfil = await _context.Perfiles
                .Include(p => p.PerfilPermisos)
                .FirstOrDefaultAsync(p => p.Id == perfilId);

            if (perfil == null) return;

            // Limpiar los permisos actuales del perfil
            _context.PerfilPermisos.RemoveRange(perfil.PerfilPermisos);

            // Asignar los nuevos permisos marcados
            if (permisosSeleccionados != null && permisosSeleccionados.Any())
            {
                foreach (var permisoId in permisosSeleccionados)
                {
                    _context.PerfilPermisos.Add(new PerfilPermiso
                    {
                        PerfilId = perfilId,
                        PermisoId = permisoId
                    });
                }
            }

            // Obtener los nombres de los permisos para el detalle de auditoría
            var nombresPermisos = await _context.Permisos
                .Where(p => permisosSeleccionados != null && permisosSeleccionados.Contains(p.Id))
                .Select(p => p.CodigoSistema)
                .ToListAsync();

            string detallePermisos = nombresPermisos.Any()
                ? string.Join(", ", nombresPermisos)
                : "Ninguno";

            // Registro de auditoría detallado
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuarioAuditor,
                Accion = "Actualización de Permisos",
                Modulo = "Perfiles",
                Fecha = DateTime.Now,
                Detalles = $"Perfil '{perfil.Nombre}' actualizado. Permisos activos: [{detallePermisos}]"
            });

            await _context.SaveChangesAsync();
        }
    }
}