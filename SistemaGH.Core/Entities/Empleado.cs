using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaGH.Core.Entities
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 80 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los nombres solo pueden contener letras y espacios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los apellidos solo pueden contener letras y espacios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "El documento debe tener entre 5 y 20 dígitos.")]
        [RegularExpression(@"^[0-9A-Za-z\-]+$", ErrorMessage = "El documento solo admite números, letras y guiones.")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El cargo debe tener entre 2 y 100 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
    }
}