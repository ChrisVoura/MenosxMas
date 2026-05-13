using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;
using Microsoft.EntityFrameworkCore;


namespace MiPrimeraWebApp.Pages
{
    public class DetallesModel : PageModel
    {
        private readonly AppDbContext _db;

        public Producto? Producto { get; set; }

        public List<Comentario> Comentarios { get; set; } = new();
        public Dictionary<int, string> NombresClientes { get; set; } = new();
        public int ClienteIdActual { get; set; }

        [BindProperty]
        public string? Descripcion {get; set;}
        [BindProperty]
        public int valor {get; set;}

        public string UsuarioRol {get; set;} = "";

        public DetallesModel(AppDbContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId") ?? "";
            if (!string.IsNullOrEmpty(usuarioId))
            {
                ClienteIdActual = int.Parse(usuarioId);
            }
            
            if (Request.Query.ContainsKey("id") && int.TryParse(Request.Query["id"], out int id))
            {
                Producto = _db.Productos.FirstOrDefault(p => p.Id == id);
                
                var comentariosToDelete = _db.Comentarios.Where(c => c.ProductoId == id && (c.Descripcion == null || c.Descripcion == "")).ToList();
                _db.Comentarios.RemoveRange(comentariosToDelete);
                _db.SaveChanges();
                
                Comentarios = _db.Comentarios.Where(c => c.ProductoId == id).ToList();
                
                foreach (var comentario in Comentarios)
                {
                    var cliente = _db.Clientes.FirstOrDefault(c => c.Id == comentario.ClienteId);
                    if (cliente != null)
                    {
                        NombresClientes[comentario.ClienteId] = $"{cliente.Name} {cliente.Apellido}";
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAgregarComentario(int ProductoId)
        {
            UsuarioRol = HttpContext.Session.GetString("UsuarioRol") ?? "";
            var usuarioId = HttpContext.Session.GetString("UsuarioId") ?? "";
            
            if (UsuarioRol != "Cliente")
            {
                return RedirectToPage("/Login");
            }
            var id = int.Parse(usuarioId);
            var nuevoComentario = new Comentario
            {
                ClienteId = id,
                ProductoId = ProductoId,
                Descripcion = Descripcion,
                valor = valor,
                FechaCreaciacion = DateTime.Now,
            };
            _db.Comentarios.Add(nuevoComentario);
            await _db.SaveChangesAsync();
            return RedirectToPage("/Detalles", new { id = ProductoId });
        }

        public async Task<IActionResult> OnPostEliminarComentario(int ComentarioId, int ProductoId)
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId") ?? "";
            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToPage("/Login");
            }
            
            var clienteIdActual = int.Parse(usuarioId);
            var comentario = _db.Comentarios.FirstOrDefault(c => c.Id == ComentarioId);
            
            if (comentario != null && comentario.ClienteId == clienteIdActual)
            {
                _db.Comentarios.Remove(comentario);
                await _db.SaveChangesAsync();
            }
            
            return RedirectToPage("/Detalles", new { id = ProductoId });
        }
    }
}