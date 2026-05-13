

namespace MiPrimeraWebApp.Data
{
    public class Pedido
    {
       public int Id { get; set; }
       public int ClienteId { get; set; }
       public Cliente? Cliente { get; set; }
       public DateTime FechaPedido { get; set; } = DateTime.Now;
       public decimal Total { get; set; } 

       public string Estado { get; set; } = "Pendiente";
    }
}