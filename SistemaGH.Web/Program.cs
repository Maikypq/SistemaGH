using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Infrastructure.Data;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Registrar el AppDbContext para la base de datos con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de servicios y repositorios (Arquitectura Limpia)
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>(); 

// cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; 
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); 
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Ajustado a AccessDenied (inglés como tu AuthController)
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// ==========================================
// SEEDING AUTOMÁTICO: Admin, Perfil y Permisos
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // 1. Crear Perfil Administrador si no existe
        var perfilAdmin = await context.Perfiles.FirstOrDefaultAsync(p => p.Nombre == "Administrador");
        if (perfilAdmin == null)
        {
            perfilAdmin = new Perfil { Nombre = "Administrador", Descripcion = "Acceso total al sistema" };
            context.Perfiles.Add(perfilAdmin);
            await context.SaveChangesAsync();
        }

        // 2. Lista base de Permisos del sistema adaptada a tu modelo
        var permisosBase = new List<(string CodigoSistema, string Modulo, string Accion)>
        {
           ("Usuarios.Ver", "Usuarios", "Ver"),
            ("Usuarios.Crear", "Usuarios", "Crear"),
            ("Usuarios.Editar", "Usuarios", "Editar"),
            ("Usuarios.Eliminar", "Usuarios", "Eliminar"),
            ("Permisos.Gestionar", "Permisos", "Gestionar"),
            // Empleados ---
            ("Empleados.Ver", "Empleados", "Ver"),
            ("Empleados.Crear", "Empleados", "Crear"),
            ("Empleados.Editar", "Empleados", "Editar"),
            ("Empleados.Eliminar", "Empleados", "Eliminar")
        };

        foreach (var p in permisosBase)
        {
            var permisoExistente = await context.Permisos.FirstOrDefaultAsync(perm => perm.CodigoSistema == p.CodigoSistema);
            if (permisoExistente == null)
            {
                permisoExistente = new Permiso 
                { 
                    CodigoSistema = p.CodigoSistema, 
                    Modulo = p.Modulo, 
                    Accion = p.Accion 
                };
                context.Permisos.Add(permisoExistente);
                await context.SaveChangesAsync();
            }

            // Asignar el permiso al perfil Administrador si no lo tiene
            bool tienePermiso = await context.PerfilPermisos
                .AnyAsync(pp => pp.PerfilId == perfilAdmin.Id && pp.PermisoId == permisoExistente.Id);

            if (!tienePermiso)
            {
                context.PerfilPermisos.Add(new PerfilPermiso
                {
                    PerfilId = perfilAdmin.Id,
                    PermisoId = permisoExistente.Id
                });
            }
        }
        await context.SaveChangesAsync();

        // 3. Crear Usuario Administrador Inicial con PasswordHasher
        var adminCorreo = "admin@lameseta.com";
        var adminUsuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Correo == adminCorreo);
        if (adminUsuario == null)
        {
            var hasher = new PasswordHasher<Usuario>();
            var nuevoAdmin = new Usuario
            {
                NombreCompleto = "Administrador Inicial",
                Correo = adminCorreo,
                Activo = true,
                PerfilId = perfilAdmin.Id
            };
            nuevoAdmin.PasswordHash = hasher.HashPassword(nuevoAdmin, "Admin123*");

            context.Usuarios.Add(nuevoAdmin);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sembrar los datos iniciales de seguridad.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();