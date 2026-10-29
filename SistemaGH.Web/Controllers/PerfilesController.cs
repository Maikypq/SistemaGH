using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Infrastructure.Data;
using SistemaGH.Web.Filters;
using SistemaGH.Web.Models;

namespace SistemaGH.Web.Controllers
{
    public class PerfilesController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilesController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var perfiles = await _context.Perfiles.ToListAsync();
            return View(perfiles);
        }

        [HttpGet]
        [RequirePermission("Permisos.Gestionar")]
        public async Task<IActionResult> AsignarPermisos(int perfilId)
        {
            var perfil = await _context.Perfiles.FindAsync(perfilId);
            if (perfil == null) return NotFound();

            var todosLosPermisos = await _context.Permisos.ToListAsync();
            var permisosDelPerfil = await _context.PerfilPermisos
                .Where(pp => pp.PerfilId == perfilId)
                .Select(pp => pp.PermisoId)
                .ToListAsync();

            var viewModel = new AsignarPermisosViewModel
            {
                PerfilId = perfil.Id,
                NombrePerfil = perfil.Nombre,
                Modulos = todosLosPermisos
                    .GroupBy(p => p.Modulo)
                    .Select(g => new ModuloPermisosGrupoViewModel
                    {
                        Modulo = g.Key,
                        Permisos = g.Select(p => new PermisoCheckboxViewModel
                        {
                            PermisoId = p.Id,
                            CodigoSistema = p.CodigoSistema,
                            Accion = p.Accion,
                            Seleccionado = permisosDelPerfil.Contains(p.Id)
                        }).ToList()
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Permisos.Gestionar")]
        public async Task<IActionResult> AsignarPermisos(int perfilId, List<int> permisosSeleccionados)
        {
            var perfil = await _context.Perfiles
                .Include(p => p.PerfilPermisos)
                .FirstOrDefaultAsync(p => p.Id == perfilId);

            if (perfil == null) return NotFound();

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

            // === OBTENER LOS NOMBRES DE LOS PERMISOS PARA EL DETALLE ===
            var nombresPermisos = await _context.Permisos
                .Where(p => permisosSeleccionados != null && permisosSeleccionados.Contains(p.Id))
                .Select(p => p.Accion) // O p.CodigoSistema, como prefieras mostrarlo
                .ToListAsync();

            string detallePermisos = nombresPermisos.Any() 
                ? string.Join(", ", nombresPermisos) 
                : "Ninguno";
            // ============================================================

            // REGISTRO DE AUDITORÍA DETALLADO
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = User.Identity?.Name ?? "Sistema",
                Accion = "Actualización de Permisos",
                Modulo = "Perfiles",
                Fecha = DateTime.Now,
                Detalles = $"Perfil '{perfil.Nombre}' actualizado. Permisos activos: [{detallePermisos}]"
            });

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Los permisos han sido actualizados exitosamente.";
            return RedirectToAction("Index");
        }
    }
}