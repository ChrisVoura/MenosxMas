namespace MiPrimeraWebApp.Data;

public class Producto
{
    public int Id { get; set; } 
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public required string? Category { get; set; }
    public required string ImageUrl { get; set; }
    public required int Stock { get; set; }
    public bool DescuentoActivo { get; set; } = false;
    public int PorcentajeDescuento { get; set; } = 50;
}
