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
            return await _context.Empleados.ToListAsync();
        }

        public async Task<Empleado?> ObtenerPorIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task AgregarAsync(Empleado empleado)
        {
            await _context.Empleados.AddAsync(empleado);
            await _context.SaveChangesAsync();
        }
    }
}