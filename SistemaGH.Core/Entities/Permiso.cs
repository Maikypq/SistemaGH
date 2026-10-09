using System.Collections.Generic;

namespace SistemaGH.Core.Entities
{
    public class Permiso
    {
        public int Id { get; set; }
        public string Modulo { get; set; } = string.Empty; // Ej: "Empleados", "Usuarios"
        public string Accion { get; set; } = string.Empty; // Ej: "Crear", "Editar", "Inactivar"
        public string CodigoSistema { get; set; } = string.Empty; // Ej: "Empleados.Crear"

        public ICollection<PerfilPermiso> PerfilPermisos { get; set; } = new List<PerfilPermiso>();
    }
}