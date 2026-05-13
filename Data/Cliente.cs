using System.ComponentModel.DataAnnotations;

namespace MiPrimeraWebApp.Data;

public  class Cliente
{
    public int Id {get ; set; }
  
    required public string Name {get; set; }

    required public string Apellido {get; set; }
    [StringLength(100)] 
    [EmailAddress]
    required public string Email {get; set; }

    required public string Password {get; set; }
    required public DateTime FechaRegistro {get; set; } = DateTime.Now;

    public string Genero { get; set; } = "No especificado";

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public DateTime? fechaNacimiento { get; set; }

    public string Rol { get; set; } = "Cliente";
}