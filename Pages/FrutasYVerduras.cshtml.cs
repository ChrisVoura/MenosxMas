using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;


namespace MiPrimeraWebApp.Pages
{
    public class FrutasYVerdurasModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<Producto> Productos { get; set; }

        public FrutasYVerdurasModel(AppDbContext db)
        {
            _db = db;
            Productos = new List<Producto>();
        }

        public void OnGet()
        {
            Productos = _db.Productos.Where(p => p.Category == "Frutas" || p.Category == "Verduras").ToList();

            if (Productos.Count == 0)
            {
                ModelState.AddModelError("Productos", "No hay productos disponibles en esta categoría.");
            }
        }

    }
}