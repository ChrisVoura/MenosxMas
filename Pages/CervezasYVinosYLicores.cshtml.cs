using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class CervezasYVinosYLicoresModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<Producto> Productos { get; set; }

        public CervezasYVinosYLicoresModel(AppDbContext db)
        {
            _db = db;
            Productos = new List<Producto>();
        }
        public void OnGet()
        {
            Productos = _db.Productos.Where(p => p.Category == "Cerveza" || p.Category == "Vino" || p.Category == "Licores").ToList();
        
            if (Productos.Count == 0)
            {
                ModelState.AddModelError("Productos", "No hay productos disponibles en esta categoría.");
            }
        }
    }
}