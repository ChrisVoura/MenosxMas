using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiPrimeraWebApp.Pages;

public class ContactoModel : PageModel
{
    [BindProperty]
    public required string Nombre { get; set; }

    [BindProperty]
    public required string Email { get; set; }

    public void OnGet()
    {

    }

    public IActionResult OnPost(){
        if (!ModelState.IsValid)
        {
            return Page();
        }
        TempData["SuccessMessage"] = $"Gracias por contactarnos {Nombre}, te responderemos a la brevedad a tu correo {Email}.";
        return RedirectToPage("/Contacto");
    }
        
}
