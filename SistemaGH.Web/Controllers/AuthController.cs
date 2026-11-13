using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Infrastructure.Data;
using SistemaGH.Web.Models;

namespace SistemaGH.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("PoliticaLogin")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var correoNormalizado = model.Correo.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNormalizado);

            if (usuario == null || !usuario.Activo)
            {
                _context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Usuario = correoNormalizado,
                    Accion = "Intento Fallido de Login",
                    Modulo = "Autenticación",
                    Fecha = DateTime.Now,
                    Detalles = "Intento de acceso con cuenta inexistente o inactiva."
                });
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta inactiva.");
                return View(model);
            }

            var hasher = new PasswordHasher<Usuario>();
            var verificationResult = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Usuario = usuario.Correo,
                    Accion = "Contraseña Incorrecta",
                    Modulo = "Autenticación",
                    Fecha = DateTime.Now,
                    Detalles = $"Intento fallido de inicio de sesión para {usuario.NombreCompleto}."
                });
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Re-hashear automáticamente si el algoritmo de Identity requiere actualización
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = hasher.HashPassword(usuario, model.Password);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("PerfilId", usuario.PerfilId.ToString()),
                new Claim(ClaimTypes.Role, usuario.Perfil?.Nombre ?? "SinPerfil")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuario.NombreCompleto,
                Accion = "Inicio de Sesión Exitoso",
                Modulo = "Autenticación",
                Fecha = DateTime.Now,
                Detalles = $"Acceso concedido con perfil '{usuario.Perfil?.Nombre}'."
            });
            await _context.SaveChangesAsync();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Protección contra Open Redirect: valida que returnUrl sea estrictamente local
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var nombreUsuario = User.Identity?.Name ?? "Usuario";
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = nombreUsuario,
                Accion = "Cierre de Sesión",
                Modulo = "Autenticación",
                Fecha = DateTime.Now,
                Detalles = "El usuario cerró su sesión de forma segura."
            });
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}