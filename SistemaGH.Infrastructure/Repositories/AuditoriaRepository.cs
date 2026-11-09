using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGH.Core.Entities;
using SistemaGH.Core.Interfaces;
using SistemaGH.Infrastructure.Data;

namespace SistemaGH.Infrastructure.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly AppDbContext _context;

        public AuditoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditoriaLog>> ObtenerTodosAsync()
        {
            return await _context.AuditoriaLogs
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task RegistrarEventoAsync(string usuario, string modulo, string accion, string detalles)
        {
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                Usuario = usuario,
                Modulo = modulo,
                Accion = accion,
                Fecha = DateTime.Now,
                Detalles = detalles
            });
            await _context.SaveChangesAsync();
        }
    }
}