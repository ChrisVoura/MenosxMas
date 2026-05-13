using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;
namespace MiPrimeraWebApp.Pages
{
    public class JugosYBebidasModel : PageModel
    {
        public List<Producto> Productos {get; set;}
        private readonly AppDbContext _db;
        public JugosYBebidasModel(AppDbContext db)
        {
            _db = db;
            Productos = new List<Producto>();
        }
        public void OnGet()
        {
            Productos = _db.Productos.Where(p => p.Category == "Bebidas" || p.Category == "Jugos").ToList();
            if (Productos.Count == 0)
            {
                ModelState.AddModelError("Productos", "No hay productos disponibles en esta categoría.");
            }
        }
    }
}