using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGH.Core.Interfaces;
using SistemaGH.Web.Models;

namespace SistemaGH.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly IAuditoriaRepository _auditoriaRepository;

        public HomeController(
            IEmpleadoRepository empleadoRepository,
            IUsuarioRepository usuarioRepository,
            IPerfilRepository perfilRepository,
            IAuditoriaRepository auditoriaRepository)
        {
            _empleadoRepository = empleadoRepository;
            _usuarioRepository = usuarioRepository;
            _perfilRepository = perfilRepository;
            _auditoriaRepository = auditoriaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = (await _empleadoRepository.ObtenerTodosAsync()).ToList();
            var usuarios = (await _usuarioRepository.ObtenerTodosAsync()).ToList();
            var perfiles = (await _perfilRepository.ObtenerTodosAsync()).ToList();
            var logs = (await _auditoriaRepository.ObtenerTodosAsync()).ToList();

            int totalEmpleados = empleados.Count;
            int totalUsuarios = usuarios.Count;
            int usuariosActivos = usuarios.Count(u => u.Activo);
            int totalPerfiles = perfiles.Count;
            int totalEventos = logs.Count;

            // Porcentaje de cobertura/actividad para el velocímetro central
            int porcentajeSalud = totalUsuarios > 0
                ? (int)Math.Round((double)usuariosActivos / totalUsuarios * 100)
                : 100;

            // Conteo de actividad por módulo para las barras verticales
            ViewBag.LogsEmpleados = logs.Count(l => l.Modulo.Contains("Empleado", StringComparison.OrdinalIgnoreCase));
            ViewBag.LogsUsuarios = logs.Count(l => l.Modulo.Contains("Usuario", StringComparison.OrdinalIgnoreCase));
            ViewBag.LogsPerfiles = logs.Count(l => l.Modulo.Contains("Perfil", StringComparison.OrdinalIgnoreCase));
            ViewBag.LogsAuth = logs.Count(l => l.Modulo.Contains("Autentic", StringComparison.OrdinalIgnoreCase));

            ViewBag.TotalEmpleados = totalEmpleados;
            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.UsuariosActivos = usuariosActivos;
            ViewBag.TotalPerfiles = totalPerfiles;
            ViewBag.TotalEventos = totalEventos;
            ViewBag.PorcentajeSalud = porcentajeSalud;

            ViewBag.UsuarioDestacado = usuarios.FirstOrDefault();
            ViewBag.SegundoUsuario = usuarios.Skip(1).FirstOrDefault() ?? usuarios.FirstOrDefault();
            ViewBag.UltimosLogs = logs.Take(4).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}