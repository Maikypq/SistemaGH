using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaGH.Core.Entities;

namespace SistemaGH.Core.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task<IEnumerable<AuditoriaLog>> ObtenerTodosAsync();
        Task RegistrarEventoAsync(string usuario, string modulo, string accion, string detalles);
    }
}