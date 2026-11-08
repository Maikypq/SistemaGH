using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Web.Filters;
using SistemaGH.Web.Models;

namespace SistemaGH.Web.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // ================= GET: Index =================
        [RequirePermission("Usuarios.Ver")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioRepository.ObtenerTodosAsync();
            return View(usuarios);
        }

        // ================= GET: Crear =================
        [HttpGet]
        [RequirePermission("Usuarios.Crear")]
        public async Task<IActionResult> Crear()
        {
            var perfiles = await _usuarioRepository.ObtenerPerfilesAsync();
            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre");
            return View(new UsuarioCrearViewModel());
        }

        // ================= POST: Crear =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Usuarios.Crear")]
        public async Task<IActionResult> Crear(UsuarioCrearViewModel model)
        {
            var existente = await _usuarioRepository.ObtenerPorCorreoAsync(model.Correo);
            if (existente != null)
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario registrado con este correo.");
            }

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<Usuario>();
                var nuevoUsuario = new Usuario
                {
                    NombreCompleto = model.NombreCompleto,
                    Correo = model.Correo,
                    PerfilId = model.PerfilId,
                    Activo = model.Activo
                };
                nuevoUsuario.PasswordHash = hasher.HashPassword(nuevoUsuario, model.Password);

                var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
                await _usuarioRepository.AgregarAsync(nuevoUsuario, usuarioActual);
                await _usuarioRepository.GuardarCambiosAsync();

                TempData["Mensaje"] = "Usuario registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            var perfiles = await _usuarioRepository.ObtenerPerfilesAsync();
            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre", model.PerfilId);
            return View(model);
        }

        // ================= GET: Editar =================
        [HttpGet]
        [RequirePermission("Usuarios.Editar")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            var model = new UsuarioEditarViewModel
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                PerfilId = usuario.PerfilId,
                Activo = usuario.Activo
            };

            var perfiles = await _usuarioRepository.ObtenerPerfilesAsync();
            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre", usuario.PerfilId);
            return View(model);
        }

        // ================= POST: Editar =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Usuarios.Editar")]
        public async Task<IActionResult> Editar(int id, UsuarioEditarViewModel model)
        {
            if (id != model.Id) return NotFound();

            var usuarioPorCorreo = await _usuarioRepository.ObtenerPorCorreoAsync(model.Correo);
            if (usuarioPorCorreo != null && usuarioPorCorreo.Id != model.Id)
            {
                ModelState.AddModelError("Correo", "Este correo ya está siendo utilizado por otro usuario.");
            }

            if (ModelState.IsValid)
            {
                var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
                if (usuario == null) return NotFound();

                usuario.NombreCompleto = model.NombreCompleto;
                usuario.Correo = model.Correo;
                usuario.PerfilId = model.PerfilId;
                usuario.Activo = model.Activo;

                if (!string.IsNullOrWhiteSpace(model.NuevaPassword))
                {
                    var hasher = new PasswordHasher<Usuario>();
                    usuario.PasswordHash = hasher.HashPassword(usuario, model.NuevaPassword);
                }

                var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
                await _usuarioRepository.ActualizarAsync(usuario, usuarioActual);
                await _usuarioRepository.GuardarCambiosAsync();

                TempData["Mensaje"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            var perfiles = await _usuarioRepository.ObtenerPerfilesAsync();
            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre", model.PerfilId);
            return View(model);
        }

        // ================= GET: Eliminar =================
        [HttpGet]
        [RequirePermission("Usuarios.Eliminar")]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // ================= POST: Confirmar Eliminación =================
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [RequirePermission("Usuarios.Eliminar")]
        public async Task<IActionResult> EliminarConfirmed(int id)
        {
            var usuarioActual = User.Identity?.Name ?? "Administrador del Sistema";
            await _usuarioRepository.EliminarAsync(id, usuarioActual);
            await _usuarioRepository.GuardarCambiosAsync();

            TempData["Mensaje"] = "Usuario eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}