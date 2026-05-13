using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiPrimeraWebApp.Data;


namespace MiPrimeraWebApp.Pages;

public class ClientesModel : PageModel
{
    [BindProperty] 
    public Cliente? NuevoCliente { get; set; } 

    private readonly AppDbContext _db;

    public List<Cliente> Clientes {get; set;}

    public string UsuarioRol { get; set; } = "";

    public ClientesModel(AppDbContext db)
    {
        _db = db;
        Clientes = new List<Cliente>();
    }

    public string? EmailPrefllenado { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email)
    {
        UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";

        if (UsuarioRol != "Administrador")
        {
            return RedirectToPage("/Index");
        }

        Clientes = await _db.Clientes.AsNoTracking().ToListAsync();
        Clientes = Clientes.OrderBy(c => c.Name).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostDelete()
    {
        var id = int.Parse(Request.Form["id"]);
        var cliente = await _db.Clientes.FindAsync(id);

        if (cliente is not null)
        {
            _db.Clientes.Remove(cliente);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Cliente '{cliente.Name}' eliminado exitosamente.";
        }

        return RedirectToPage("/Clientes");
    }
}