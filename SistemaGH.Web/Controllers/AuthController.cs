using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SistemaGH.Web.Controllers
{
    public class AuthController : Controller
    {
        // GET: Muestra la pantalla de login
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya tiene sesión, mandarlo al inicio para que no vuelva a ver el login
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Recibe los datos del formulario
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            // TODO: Aquí llamaremos a la base de datos más adelante. 
            // Por ahora simulamos que el login es exitoso si escribe esto:
            if (correo == "admin@lameseta.com" && password == "1234")
            {
                // 1. Crear los "Claims" (Datos del usuario que guardará la Cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, correo),
                    new Claim(ClaimTypes.Role, "Administrador")
                };

                // 2. Crear la identidad y el principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 3. Generar la Cookie de sesión
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // 4. Redirigir a la pantalla principal
                return RedirectToAction("Index", "Home");
            }

            // Si falla, recargar la vista con un error
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // GET: Cerrar sesión
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}