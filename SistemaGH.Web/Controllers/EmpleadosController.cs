using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Web.Filters;

namespace SistemaGH.Web.Controllers
{
    [Authorize] // Exige que el usuario haya iniciado sesión obligatoriamente
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoRepository _empleadoRepository;

        public EmpleadosController(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        // ================= GET: Index =================
        [RequirePermission("Empleados.Ver")]
        public async Task<IActionResult> Index()
        {
            var empleados = await _empleadoRepository.ObtenerTodosAsync();
            return View(empleados);
        }

        // ================= GET: Crear =================
        [RequirePermission("Empleados.Crear")]
        public IActionResult Crear()
        {
            return View();
        }

        // ================= POST: Crear =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Empleados.Crear")]
        public async Task<IActionResult> Crear(Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
                await _empleadoRepository.AgregarAsync(empleado, usuarioActual);
                await _empleadoRepository.GuardarCambiosAsync();
                TempData["Mensaje"] = "Empleado registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // ================= GET: Editar =================
        [HttpGet]
        [RequirePermission("Empleados.Editar")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _empleadoRepository.ObtenerPorIdAsync(id.Value);
            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // ================= POST: Editar =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Empleados.Editar")]
        public async Task<IActionResult> Editar(int id, Empleado empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
                await _empleadoRepository.ActualizarAsync(empleado, usuarioActual);
                await _empleadoRepository.GuardarCambiosAsync();
                TempData["Mensaje"] = "Empleado actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // ================= GET: Eliminar (Muestra la vista de confirmación) =================
        [HttpGet]
        [RequirePermission("Empleados.Eliminar")]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _empleadoRepository.ObtenerPorIdAsync(id.Value);
            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // ================= POST: Confirmar Eliminación =================
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [RequirePermission("Empleados.Eliminar")]
        public async Task<IActionResult> EliminarConfirmed(int id)
        {
            var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
            await _empleadoRepository.EliminarAsync(id, usuarioActual);
            await _empleadoRepository.GuardarCambiosAsync();
            TempData["Mensaje"] = "Empleado eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}