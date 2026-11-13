using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Data;
using SistemaGH.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. PROTECCIÓN DE BASE DE DATOS Y CONSULTAS PARAMETRIZADAS (EF Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// 2. PROTECCIÓN CSRF GLOBAL Y VALIDACIÓN DE MODELOS EN SERVIDOR
builder.Services.AddControllersWithViews(options =>
{
    // Exige token Anti-CSRF automáticamente en todos los métodos POST, PUT y DELETE
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = ".LaMeseta.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// 3. ENLACES DE RECUPERACIÓN Y TOKENS CON CADUCIDAD ESTRICTA
builder.Services.AddDataProtection()
    .SetApplicationName("SistemaGH_LaMeseta")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(30));

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    // Cualquier token o enlace de recuperación caduca a los 15 minutos
    options.TokenLifespan = TimeSpan.FromMinutes(15);
});

// 4. INYECCIÓN DE REPOSITORIOS (ARQUITECTURA LIMPIA)
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();

// 5. COOKIES DE SESIÓN BLINDADAS Y AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".LaMeseta.Session";
        options.Cookie.HttpOnly = true; // Impide robo de sesión por JavaScript (XSS)
        options.Cookie.SameSite = SameSiteMode.Strict; // Bloquea envío de cookie desde sitios externos (CSRF)
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always; // En producción exige HTTPS obligatorio

        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 6. RESTRICCIÓN DE CORS (Solo permite orígenes autorizados de la empresa)
var allowedOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://lameseta.com", "https://localhost:7085", "http://localhost:5085" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCorsLaMeseta", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "X-CSRF-TOKEN");
    });
});

// 7. LÍMITE DE PETICIONES (RATE LIMITING CONTRA FUERZA BRUTA Y DDoS)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Límite estricto para Login: máximo 5 intentos por minuto por IP
    options.AddPolicy("PoliticaLogin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Límite general por IP: 100 peticiones por minuto
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 2
            }));
});

var app = builder.Build();

// 8. SEEDING SEGURO DE ROLES, PERMISOS Y ADMINISTRADOR
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    // Crear perfiles base si no existen
    var perfilAdmin = context.Perfiles.FirstOrDefault(p => p.Nombre == "Administrador");
    if (perfilAdmin == null)
    {
        perfilAdmin = new Perfil { Nombre = "Administrador", Descripcion = "Acceso total y administración de seguridad" };
        context.Perfiles.Add(perfilAdmin);
        context.SaveChanges();
    }

    var perfilOperador = context.Perfiles.FirstOrDefault(p => p.Nombre == "Operador");
    if (perfilOperador == null)
    {
        perfilOperador = new Perfil { Nombre = "Operador", Descripcion = "Gestión operativa de empleados y consultas de GH" };
        context.Perfiles.Add(perfilOperador);
        context.SaveChanges();
    }

    // Sembrar permisos del sistema
    var permisosRequeridos = new List<Permiso>
    {
        new Permiso { CodigoSistema = "Usuarios.Ver", Modulo = "Usuarios", Accion = "Ver Usuarios" },
        new Permiso { CodigoSistema = "Usuarios.Crear", Modulo = "Usuarios", Accion = "Crear Usuarios" },
        new Permiso { CodigoSistema = "Usuarios.Editar", Modulo = "Usuarios", Accion = "Editar Usuarios" },
        new Permiso { CodigoSistema = "Usuarios.Eliminar", Modulo = "Usuarios", Accion = "Eliminar Usuarios" },
        new Permiso { CodigoSistema = "Permisos.Gestionar", Modulo = "Seguridad", Accion = "Gestionar Permisos y Auditoría" },
        new Permiso { CodigoSistema = "Empleados.Ver", Modulo = "Empleados", Accion = "Ver Empleados" },
        new Permiso { CodigoSistema = "Empleados.Crear", Modulo = "Empleados", Accion = "Crear Empleados" },
        new Permiso { CodigoSistema = "Empleados.Editar", Modulo = "Empleados", Accion = "Editar Empleados" },
        new Permiso { CodigoSistema = "Empleados.Eliminar", Modulo = "Empleados", Accion = "Eliminar Empleados" }
    };

    foreach (var perm in permisosRequeridos)
    {
        if (!context.Permisos.Any(p => p.CodigoSistema == perm.CodigoSistema))
        {
            context.Permisos.Add(perm);
        }
    }
    context.SaveChanges();

    // Asignar todos los permisos al Administrador
    var todosPermisos = context.Permisos.ToList();
    foreach (var perm in todosPermisos)
    {
        if (!context.PerfilPermisos.Any(pp => pp.PerfilId == perfilAdmin.Id && pp.PermisoId == perm.Id))
        {
            context.PerfilPermisos.Add(new PerfilPermiso { PerfilId = perfilAdmin.Id, PermisoId = perm.Id });
        }
    }

    // Asignar permisos operativos básicos al perfil Operador si aún no tiene ninguno
    if (!context.PerfilPermisos.Any(pp => pp.PerfilId == perfilOperador.Id))
    {
        var codigosOperador = new[] { "Empleados.Ver", "Empleados.Crear", "Empleados.Editar" };
        var permisosOperador = todosPermisos.Where(p => codigosOperador.Contains(p.CodigoSistema)).ToList();
        foreach (var perm in permisosOperador)
        {
            context.PerfilPermisos.Add(new PerfilPermiso { PerfilId = perfilOperador.Id, PermisoId = perm.Id });
        }
    }
    context.SaveChanges();

    // Crear usuario Administrador leyendo clave desde Variables de Entorno / User Secrets
    if (!context.Usuarios.Any(u => u.Correo == "admin@lameseta.com"))
    {
        var adminPassword = builder.Configuration["Security:InitialAdminPassword"] ?? "Admin123*";
        var hasher = new PasswordHasher<Usuario>();
        var adminUser = new Usuario
        {
            NombreCompleto = "Administrador Inicial",
            Correo = "admin@lameseta.com",
            Activo = true,
            PerfilId = perfilAdmin.Id
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);
        context.Usuarios.Add(adminUser);
        context.SaveChanges();
    }
}

// 9. FORZAR HTTPS Y CABECERAS DE SEGURIDAD (SECURITY HEADERS)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Fuerza HTTPS estricto en navegadores por 30 días
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none';";
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseCors("PoliticaCorsLaMeseta");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();