using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaGH.Core.Entities;

namespace SistemaGH.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<IEnumerable<Perfil>> ObtenerPerfilesAsync();
        Task AgregarAsync(Usuario usuario, string usuarioAuditor);
        Task ActualizarAsync(Usuario usuario, string usuarioAuditor);
        Task EliminarAsync(int id, string usuarioAuditor);
        Task GuardarCambiosAsync();
    }
}