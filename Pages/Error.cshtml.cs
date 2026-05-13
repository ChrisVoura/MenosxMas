using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiPrimeraWebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string UsuarioRol { get; set; } = "";
    public IActionResult OnGet()
    {
        UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";

        if (UsuarioRol != "Administrador")
        {
            return RedirectToPage("/Index");
        }
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return Page();
    }
}

