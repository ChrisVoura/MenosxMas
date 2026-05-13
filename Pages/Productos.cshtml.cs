using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class ProductosModel : PageModel
    {
        private readonly AppDbContext _db;
        [BindProperty]
        public string? Nombre { get; set; }

        [BindProperty]
        public decimal Precio { get; set; }

        [BindProperty]
        public string? Descripcion { get; set; }

        [BindProperty]
        public string? Categoria { get; set; }

        [BindProperty]
        public string? ImageUrl1 { get; set; }

        [BindProperty]
        public string? ImageUrl2 { get; set; }

        [BindProperty]
        public string? ImageUrl3 { get; set; }

        [BindProperty]
        public int Stock { get; set; }

        [BindProperty]
        public bool DescuentoActivo { get; set; } = false;

        [BindProperty]
        public int PorcentajeDescuento { get; set; } = 50;

        public List<Producto> Productos { get; set; }

        public string UsuarioRol { get; set; } = "";

        public ProductosModel(AppDbContext db)
        {
            _db = db;
            Productos = new List<Producto>();
        }
        public IActionResult OnGet()
        {
            UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";
            
            if (UsuarioRol != "Administrador" && UsuarioRol != "Empleado")
            {
                return RedirectToPage("/Index");
            }
            Productos = _db.Productos.ToList();

            if (Productos.Count == 0)
            {
                ModelState.AddModelError("Productos", "No hay productos disponibles.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                Productos = _db.Productos.ToList();
                return Page();
            }
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido.");
                Productos = _db.Productos.ToList();
                return Page();
            }
            var productoExistente = _db.Productos.FirstOrDefault(p => p.Name.ToLower() == Nombre.ToLower());
            if (productoExistente != null)
            {
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");
                Productos = _db.Productos.ToList();
                return Page();
            }

            if (Stock < 0)
            {
                ModelState.AddModelError("Stock", "El stock no puede ser negativo.");
                Productos = _db.Productos.ToList();
                return Page();
            }

            // Combina las 3 URLs en un string separado por comas
            var imageUrls = new List<string>();
            if (!string.IsNullOrWhiteSpace(ImageUrl1)) imageUrls.Add(ImageUrl1);
            if (!string.IsNullOrWhiteSpace(ImageUrl2)) imageUrls.Add(ImageUrl2);
            if (!string.IsNullOrWhiteSpace(ImageUrl3)) imageUrls.Add(ImageUrl3);
            var combinedImageUrl = string.Join(",", imageUrls);

            var nuevoProducto = new Producto
            {
                Name = Nombre ?? string.Empty,
                Price = Precio,
                Description = Descripcion ?? string.Empty,
                Category = Categoria ?? string.Empty,
                ImageUrl = combinedImageUrl,
                Stock = Stock
            };
            _db.Productos.Add(nuevoProducto);
            await _db.SaveChangesAsync();
            return RedirectToPage("/Productos");
        }

        public IActionResult OnPostDelete(int id)
        {
            var producto = _db.Productos.Find(id);
            if (producto != null)
            {
                _db.Productos.Remove(producto);
                _db.SaveChanges();
            }
            return RedirectToPage("/Productos");
        }

        public IActionResult OnPostEditar(int id, string? Nombre, decimal Precio, string? Descripcion, string? Categoria, string? ImageUrl1, string? ImageUrl2, string? ImageUrl3, int Stock, string? DescuentoActivoCheckbox, int PorcentajeDescuento)
        {
            var producto = _db.Productos.Find(id);
            if (producto != null)
            {
                producto.Name = Nombre ?? producto.Name;
                producto.Price = Precio;
                producto.Description = Descripcion ?? producto.Description;
                producto.Category = Categoria ?? producto.Category;
                
                var imageUrls = new List<string>();
                if (!string.IsNullOrWhiteSpace(ImageUrl1)) imageUrls.Add(ImageUrl1);
                if (!string.IsNullOrWhiteSpace(ImageUrl2)) imageUrls.Add(ImageUrl2);
                if (!string.IsNullOrWhiteSpace(ImageUrl3)) imageUrls.Add(ImageUrl3);
                producto.ImageUrl = string.Join(",", imageUrls);
                producto.Stock = Stock;
                producto.DescuentoActivo = DescuentoActivoCheckbox == "on";
                producto.PorcentajeDescuento = PorcentajeDescuento > 0 ? PorcentajeDescuento : 50;
                _db.SaveChanges();
            }
            return RedirectToPage("/Productos");
        }

        public IActionResult OnPostDescuento(int id)
        {
            var producto = _db.Productos.Find(id);
            if (producto != null)
            {
                producto.DescuentoActivo = !producto.DescuentoActivo;
                _db.SaveChanges();
            }
            return new JsonResult(new { success = true, descuentoActivo = producto?.DescuentoActivo });
        }

        public IActionResult OnGetObtenerStock(int id)
        {
            var producto = _db.Productos.Find(id);
            if (producto == null)
            {
                return new JsonResult(new { stock = 0 });
            }
            return new JsonResult(new { stock = producto.Stock });
        }
    }
}