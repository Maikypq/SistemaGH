namespace SistemaGH.Core.Entities
{
    public class AuditoriaLog
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Detalles { get; set; } = string.Empty;
    }
}