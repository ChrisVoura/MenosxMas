using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;


namespace MiPrimeraWebApp.Pages
{
    public class BuscarModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<Producto> Resultados { get; set; }

        public BuscarModel(AppDbContext db)
        {
            _db = db;
            Resultados = new List<Producto>();
        }
        public void OnGet()
        {
            if (Request.Query.ContainsKey("query"))
            {
                string query = Request.Query["query"].ToString().ToLower();
                Resultados = _db.Productos
                    .Where(p => p.Name.ToLower().Contains(query)).ToList();

                if (Resultados.Count == 0)
                {
                    ModelState.AddModelError("Resultados", "No se encontraron productos que coincidan con tu búsqueda.");
                }
            }
            else
            {
                ModelState.AddModelError("Resultados", "Por favor ingresa un término de búsqueda.");
            }
        }
    }
}