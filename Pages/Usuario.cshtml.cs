using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;
using System.Text.RegularExpressions;

namespace MiPrimeraWebApp.Pages
{
    public class UsuarioModel : PageModel
    {
        private readonly AppDbContext _db;

        public UsuarioModel(AppDbContext db)
        {
            _db = db;
        }
        
        public string EmailPrefllenado { get; set; } = "";
        
        public void OnGet(string? email)
        {
            EmailPrefllenado = email ?? "";
        }

        public async Task<IActionResult> OnPost()
        {
            var nombre = Request.Form["Name"].ToString();
            var apellido = Request.Form["Apellido"].ToString();
            var emailIngresado = Request.Form["Email"].ToString();
            var passwordIngresado = Request.Form["Password"].ToString();

            if (!ValidarPassword(passwordIngresado))
            {
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 12 caracteres, una mayúscula, un número y un carácter especial");
                return Page();
            }
            
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordIngresado);

            var existe = _db.Clientes.Any(c => c.Email != null && c.Email.ToLower() == emailIngresado.ToLower());
            
            if (existe)
            {
                ModelState.AddModelError("Email", "El email ya está registrado");
                return Page();
            }

            var nuevoCliente = new Cliente
            {
                Name = nombre,
                Apellido = apellido,
                Email = emailIngresado,
                Password = passwordHash,
                FechaRegistro = DateTime.Now,
                Rol = "Cliente"
            };

            _db.Clientes.Add(nuevoCliente);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("UsuarioId", nuevoCliente.Id.ToString());
            HttpContext.Session.SetString("UsuarioNombre", $"{nombre} {apellido}");
            HttpContext.Session.SetString("UsuarioRol", "Cliente");
            return RedirectToPage("/Cuentas");
        }

        private bool ValidarPassword(string password)
        {
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{12,}$");
            return regex.IsMatch(password);
        }
    }
}
