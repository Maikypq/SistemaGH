namespace SistemaGH.Web.Models
{
    public class AsignarPermisosViewModel
    {
        public int PerfilId { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
        
        // Permisos agrupados automáticamente por Módulo
        public List<ModuloPermisosGrupoViewModel> Modulos { get; set; } = new();
    }

    public class ModuloPermisosGrupoViewModel
    {
        public string Modulo { get; set; } = string.Empty;
        public List<PermisoCheckboxViewModel> Permisos { get; set; } = new();
    }

    public class PermisoCheckboxViewModel
    {
        public int PermisoId { get; set; }
        public string CodigoSistema { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public bool Seleccionado { get; set; }
    }
}