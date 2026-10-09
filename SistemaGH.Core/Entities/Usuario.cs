namespace SistemaGH.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Nunca en texto plano
        public bool Activo { get; set; } = true;

        // Relación con el Perfil
        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; } = null!;
    }
}