using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _db;

        public int TotalClientes { get; set; }
        public int TotalEmpleados { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public string UsuarioRol { get; set; } = "";

        public DashboardModel(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet()
        {
            UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";
            
            if (UsuarioRol != "Administrador")
            {
                return RedirectToPage("/Index");
            }
            
            TotalClientes = _db.Clientes.Count(c => c.Rol == "Cliente");
            TotalEmpleados = _db.Empleados.Count( e => e.Rol == "Empleado");
            TotalProductos = _db.Productos.Count();
            ProductosStockBajo = _db.Productos.Count(p => p.Stock < 10);
            
            return Page();
        }
    }
}