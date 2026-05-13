using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;

namespace MiPrimeraWebApp.Pages
{
    public class LoginPersonalModel : PageModel
    {
        private readonly AppDbContext _db;

        public LoginPersonalModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string EmailVal { get; set; } = "";
        public bool ShowPassword { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostVerificar()
        {
            var emailIngresado = Request.Form["Email"].ToString();
            
            var cliente = _db.Clientes.FirstOrDefault(c => 
                c.Email != null && c.Email.ToLower() == emailIngresado.ToLower());
            
            if (cliente != null)
            {
                ViewData["ErrorMessage"] = "Este login es solo para personal de la empresa.";
                return Page();
            }
            
            var empleado = _db.Empleados.FirstOrDefault(e => 
                e.Email != null && 
                e.Email.ToLower() == emailIngresado.ToLower() &&
                (e.Rol == "Administrador" || e.Rol == "Empleado"));
            
            if (empleado != null)
            {
                EmailVal = emailIngresado;
                ShowPassword = true;
                return Page();
            }
            
            ViewData["ErrorMessage"] = "Email no encontrado o no tiene acceso";
            return Page();
        }

        public IActionResult OnPostLogin()
        {
            var emailIngresado = Request.Form["Email"].ToString();
            var passwordIngresado = Request.Form["Password"].ToString();

            var empleado = _db.Empleados.FirstOrDefault(e => 
                e.Email != null && 
                e.Email.ToLower() == emailIngresado.ToLower() &&
                (e.Rol == "Administrador" || e.Rol == "Empleado"));

            if (empleado != null && BCrypt.Net.BCrypt.Verify(passwordIngresado, empleado.Password))
            {
                HttpContext.Session.SetString("UsuarioId", empleado.Id.ToString());
                HttpContext.Session.SetString("UsuarioNombre", $"{empleado.Nombre} {empleado.Apellido}");
                HttpContext.Session.SetString("UsuarioRol", empleado.Rol);

                if (empleado.Rol == "Administrador")
                {
                    return RedirectToPage("/Dashboard");
                }
                else
                {
                    return RedirectToPage("/DashboardEmpleado");
                }
            }

            EmailVal = emailIngresado;
            ShowPassword = true;
            ViewData["ErrorMessage"] = "Email o contraseña incorrectos";
            return Page();
        }
    }
}