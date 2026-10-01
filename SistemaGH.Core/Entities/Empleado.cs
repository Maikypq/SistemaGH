namespace SistemaGH.Core.Entities
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}