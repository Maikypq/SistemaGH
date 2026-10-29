using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Data;

namespace SistemaGH.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly AppDbContext _context;

        public EmpleadoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Empleado>> ObtenerTodosAsync()
        {
            return await _context.Empleados.Where(e => e.Activo).ToListAsync();
        }

        public async Task<Empleado?> ObtenerPorIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task AgregarAsync(Empleado empleado, string usuario)
        {
            await _context.Empleados.AddAsync(empleado);
            
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuario,
                Accion = "Creación de Empleado",
                Modulo = "Empleados",
                Fecha = DateTime.Now,
                Detalles = $"Se registró al empleado {empleado.Nombres} {empleado.Apellidos}"
            });
        }

        public async Task ActualizarAsync(Empleado empleado, string usuario)
        {
            _context.Empleados.Update(empleado);

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuario,
                Accion = "Edición de Empleado",
                Modulo = "Empleados",
                Fecha = DateTime.Now,
                Detalles = $"Se actualizó al empleado {empleado.Nombres} {empleado.Apellidos}"
            });

            await Task.CompletedTask;
        }

        public async Task EliminarAsync(int id, string usuario)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);

                _context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Usuario = usuario,
                    Accion = "Eliminación de Empleado",
                    Modulo = "Empleados",
                    Fecha = DateTime.Now,
                    Detalles = $"Se eliminó al empleado {empleado.Nombres} {empleado.Apellidos}"
                });
            }
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}