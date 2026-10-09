using System.Collections.Generic;

namespace SistemaGH.Core.Entities
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<PerfilPermiso> PerfilPermisos { get; set; } = new List<PerfilPermiso>();
    }
}