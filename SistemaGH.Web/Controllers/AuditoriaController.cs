using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Infrastructure.Data;
using SistemaGH.Web.Filters;

namespace SistemaGH.Web.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [RequirePermission("Permisos.Gestionar")] // O el permiso que queramos restringir
        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditoriaLogs
                .OrderByDescending(l => l.Fecha)
                .Take(100) // Mostramos los últimos 100 eventos
                .ToListAsync();

            return View(logs);
        }
    }
}