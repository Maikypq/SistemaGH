using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Data;

namespace SistemaGH.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Perfil)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }

        public async Task<IEnumerable<Perfil>> ObtenerPerfilesAsync()
        {
            return await _context.Perfiles.ToListAsync();
        }

        public async Task AgregarAsync(Usuario usuario, string usuarioAuditor)
        {
            await _context.Usuarios.AddAsync(usuario);

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuarioAuditor,
                Accion = "Creación de Usuario",
                Modulo = "Usuarios",
                Fecha = DateTime.Now,
                Detalles = $"Se registró al usuario {usuario.NombreCompleto} ({usuario.Correo})"
            });
        }

        public async Task ActualizarAsync(Usuario usuario, string usuarioAuditor)
        {
            _context.Usuarios.Update(usuario);

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuarioAuditor,
                Accion = "Edición de Usuario",
                Modulo = "Usuarios",
                Fecha = DateTime.Now,
                Detalles = $"Se actualizó al usuario {usuario.NombreCompleto} ({usuario.Correo})"
            });

            await Task.CompletedTask;
        }

        public async Task EliminarAsync(int id, string usuarioAuditor)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);

                _context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Usuario = usuarioAuditor,
                    Accion = "Eliminación de Usuario",
                    Modulo = "Usuarios",
                    Fecha = DateTime.Now,
                    Detalles = $"Se eliminó al usuario {usuario.NombreCompleto} ({usuario.Correo})"
                });
            }
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}