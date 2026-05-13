using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class EmpleadosModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<Empleado> listaEmpleados { get; set; }

        [BindProperty]
        public NuevoEmpleadoInput NuevoEmpleado { get; set; } = new();
        
        public string UsuarioRol { get; set; } = "";

        public EmpleadosModel(AppDbContext db)
        {
            _db = db;
            listaEmpleados = new List<Empleado>();
        }

        public IActionResult OnGet()
        {
            UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";

            if (UsuarioRol != "Administrador")
            {
                return RedirectToPage("/Index");
            }
            listaEmpleados = _db.Empleados.ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                listaEmpleados = _db.Empleados.ToList();
                return Page();
            }

            var empleado = new Empleado
            {
                Nombre = NuevoEmpleado.Nombre,
                Apellido = NuevoEmpleado.Apellido,
                Email = NuevoEmpleado.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(NuevoEmpleado.Password),
                Rol = NuevoEmpleado.Rol,
                Salario = NuevoEmpleado.Salario,
                Puesto = NuevoEmpleado.Puesto
            };

            _db.Empleados.Add(empleado);
            _db.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostEliminar()
        {
            var id = int.Parse(Request.Form["id"]);
            var empleado = _db.Empleados.Find(id);
            if (empleado != null)
            {
                _db.Empleados.Remove(empleado);
                _db.SaveChanges();
                TempData["Success"] = $"Empleado '{empleado.Nombre}' eliminado exitosamente.";
            }
            return RedirectToPage();
        }
    }

    public class NuevoEmpleadoInput
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Rol { get; set; } = "Empleado";
        public decimal Salario { get; set; }
        public string? Puesto { get; set; }
    }
}