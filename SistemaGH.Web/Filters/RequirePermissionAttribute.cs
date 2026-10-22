using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Infrastructure.Data;
using System.Security.Claims;

namespace SistemaGH.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string codigoSistema) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { codigoSistema };
        }
    }

    public class RequirePermissionFilter : IAsyncActionFilter
    {
        private readonly string _codigoSistema;
        private readonly AppDbContext _context;

        public RequirePermissionFilter(string codigoSistema, AppDbContext context)
        {
            _codigoSistema = codigoSistema;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. Validar si el usuario está autenticado
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // 2. Obtener el PerfilId de los Claims del usuario autenticado
            var perfilIdClaim = user.FindFirst("PerfilId")?.Value;

            if (string.IsNullOrEmpty(perfilIdClaim) || !int.TryParse(perfilIdClaim, out int perfilId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            // 3. Validar en la Base de Datos si el perfil tiene asignado el permiso
            bool tienePermiso = await _context.PerfilPermisos
                .Include(pp => pp.Permiso)
                .AnyAsync(pp => pp.PerfilId == perfilId && pp.Permiso.CodigoSistema == _codigoSistema);

            if (!tienePermiso)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            // Si tiene el permiso, continúa con la ejecución normal
            await next();
        }
    }
}