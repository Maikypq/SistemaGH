using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaGH.Core.Entities;

namespace SistemaGH.Core.Interfaces
{
    public interface IPerfilRepository
    {
        Task<IEnumerable<Perfil>> ObtenerTodosAsync();
        Task<Perfil?> ObtenerPorIdConPermisosAsync(int id);
        Task<IEnumerable<Permiso>> ObtenerTodosLosPermisosAsync();
        Task ActualizarPermisosAsync(int perfilId, List<int> permisosSeleccionados, string usuarioAuditor);
    }
}