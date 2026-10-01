using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaGH.Core.Entities;

namespace SistemaGH.Core.Interfaces
{
    public interface IEmpleadoRepository
    {
        Task<IEnumerable<Empleado>> ObtenerTodosAsync();
        Task<Empleado?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Empleado empleado);
    }
}