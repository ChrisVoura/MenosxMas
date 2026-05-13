using System.ComponentModel.DataAnnotations;

namespace MiPrimeraWebApp.Data;

public class ListaDeseo
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public string? Productos { get; set; }
}