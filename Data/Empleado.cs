using System;
using System.ComponentModel.DataAnnotations;

namespace MiPrimeraWebApp.Data
{
    public class Empleado
    {
        public int Id { get; set; }
        
        [Required]
        public string Nombre { get; set; } = "";

        [Required]
        public string Apellido { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "Empleado";

        public DateTime FechaContratacion { get; set; } = DateTime.Now;
        public DateTime? FechaDespido { get; set; }
        public decimal Salario { get; set; }
        public string? Puesto { get; set; }
    }
}