using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGH.Core.Interfaces;
using SistemaGH.Web.Filters;
using SistemaGH.Web.Models;

namespace SistemaGH.Web.Controllers
{
    [Authorize]
    public class PerfilesController : Controller
    {
        private readonly IPerfilRepository _perfilRepository;

        public PerfilesController(IPerfilRepository perfilRepository)
        {
            _perfilRepository = perfilRepository;
        }

        [HttpGet]
        [RequirePermission("Permisos.Gestionar")]
        public async Task<IActionResult> Index()
        {
            var perfiles = await _perfilRepository.ObtenerTodosAsync();
            return View(perfiles);
        }

        [HttpGet]
        [RequirePermission("Permisos.Gestionar")]
        public async Task<IActionResult> AsignarPermisos(int perfilId)
        {
            var perfil = await _perfilRepository.ObtenerPorIdConPermisosAsync(perfilId);
            if (perfil == null) return NotFound();

            var todosLosPermisos = await _perfilRepository.ObtenerTodosLosPermisosAsync();
            var permisosDelPerfil = perfil.PerfilPermisos
                .Select(pp => pp.PermisoId)
                .ToList();

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
            var perfil = await _perfilRepository.ObtenerPorIdConPermisosAsync(perfilId);
            if (perfil == null) return NotFound();

            var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
            await _perfilRepository.ActualizarPermisosAsync(perfilId, permisosSeleccionados, usuarioActual);

            TempData["Mensaje"] = "Los permisos han sido actualizados exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}