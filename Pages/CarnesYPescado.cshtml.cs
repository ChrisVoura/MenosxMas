using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class CarnesYPescadoModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<Producto> Productos { get; set; }

        public CarnesYPescadoModel(AppDbContext db)
        {
            _db = db;
            Productos = new List<Producto>();
        }

        public void OnGet()
        {
            Productos = _db.Productos.Where(p => p.Category == "Carnes" || p.Category == "Pescados").ToList();

            if (Productos.Count == 0)
            {
                ModelState.AddModelError("Productos", "No hay productos disponibles en esta categoría.");
            }
        }
    }
}