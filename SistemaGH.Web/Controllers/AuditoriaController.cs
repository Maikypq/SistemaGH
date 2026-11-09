using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGH.Core.Interfaces;
using SistemaGH.Web.Filters;

namespace SistemaGH.Web.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaRepository _auditoriaRepository;

        public AuditoriaController(IAuditoriaRepository auditoriaRepository)
        {
            _auditoriaRepository = auditoriaRepository;
        }

        [RequirePermission("Permisos.Gestionar")]
        public async Task<IActionResult> Index()
        {
            var logs = await _auditoriaRepository.ObtenerTodosAsync();
            return View(logs);
        }
    }
}