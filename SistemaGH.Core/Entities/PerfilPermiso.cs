namespace SistemaGH.Core.Entities
{
    public class PerfilPermiso
    {
        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; } = null!;

        public int PermisoId { get; set; }
        public Permiso Permiso { get; set; } = null!;
    }
}