using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiPrimeraWebApp.Data;


namespace MiPrimeraWebApp.Pages
{
    public class ListaDeseoConProductos
    {
        public ListaDeseo Lista { get; set; } = null!;
        public List<Producto> Productos { get; set; } = new();
    }

    public class CuentasModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<ListaDeseoConProductos>? ListasDeseos { get; set; }

        public List<Pedido>? Pedidos { get; set; }
        public List<string>? Direcciones { get; set; }

        public CuentasModel(AppDbContext db)
        {
            _db = db;
        }
        
        [BindProperty]
        public string? NombreUsuario { get; set; }
        [BindProperty]
        public string? Nombre { get; set; }
        [BindProperty]
        public string? Apellido { get; set; }
        [BindProperty]
        public string? Email { get; set; }
        [BindProperty]
        public string? Genero { get; set; }
        [BindProperty]
        public string? Telefono { get; set; }
        [BindProperty]
        public string? Direccion { get; set; }
        [BindProperty]
        public string? NuevaDireccion { get; set; }
        [BindProperty]
        public DateTime? FechaNacimiento { get; set; }

        [BindProperty]
        public int? ProductoId { get; set; }

        [BindProperty]
        public string? NuevaListaNombre { get; set; }

        [BindProperty]
        public int? ListaId { get; set; }

        public void OnGet()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                Response.Redirect("/Login");
                return;
            }
            
            var id = int.Parse(usuarioId);
            var cliente = _db.Clientes.Find(id);
            
            if (cliente == null)
            {
                Response.Redirect("/Login");
                return;
            }
            
            NombreUsuario = $"{cliente.Name} {cliente.Apellido}";
            Nombre = cliente.Name;
            Apellido = cliente.Apellido;
            Email = cliente.Email;
            Genero = cliente.Genero;
            Telefono = cliente.Telefono;
            FechaNacimiento = cliente.fechaNacimiento;
            Direcciones = string.IsNullOrEmpty(cliente.Direccion) 
                ? new List<string>() 
                : new List<string> { cliente.Direccion };

            var listas = _db.ListasDeseos.Where(l => l.ClienteId == id).ToList();
            ListasDeseos = listas.Select(l => {
                var productosIds = string.IsNullOrEmpty(l.Productos)
                    ? new List<int>()
                    : l.Productos.Split(',').Select(int.Parse).ToList();
                return new ListaDeseoConProductos
                {
                    Lista = l,
                    Productos = _db.Productos.Where(p => productosIds.Contains(p.Id)).ToList()
                };
            }).ToList();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostObtenerListas()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return new JsonResult(new { success = false, message = "Debes iniciar sesión." });
            }

            var id = int.Parse(usuarioId);
            var listas = _db.ListasDeseos.Where(l => l.ClienteId == id)
                .Select(l => new { id = l.Id, nombre = l.Nombre })
                .ToList();

            return new JsonResult(listas);
        }

        public IActionResult OnPostCrearLista()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return new JsonResult(new { success = false, message = "Debes iniciar sesión." });
            }

            var id = int.Parse(usuarioId);

            if (!string.IsNullOrEmpty(NuevaListaNombre))
            {
                var lista = new ListaDeseo
                {
                    Nombre = NuevaListaNombre,
                    ClienteId = id,
                    Productos = ""
                };
                _db.ListasDeseos.Add(lista);
                _db.SaveChanges();
            }

            return RedirectToPage("/Cuentas");
        }

        public IActionResult OnPostAgregarAListaEspecifica()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return new JsonResult(new { success = false, message = "Debes iniciar sesión." });
            }

            if (ProductoId.HasValue && ListaId.HasValue)
            {
                var lista = _db.ListasDeseos.Find(ListaId.Value);
                if (lista != null)
                {
                    var productosActual = string.IsNullOrEmpty(lista.Productos)
                        ? new List<int>()
                        : lista.Productos.Split(',').Select(int.Parse).ToList();

                    if (!productosActual.Contains(ProductoId.Value))
                    {
                        productosActual.Add(ProductoId.Value);
                        lista.Productos = string.Join(",", productosActual);
                        _db.ListasDeseos.Attach(lista);
                        var entry = _db.Entry(lista);
                        entry.Property(l => l.Productos).IsModified = true;
                        _db.SaveChanges();
                    }
                }
            }

            return new JsonResult(new { success = true });
        }

        public IActionResult OnPostAgregarListaAlCarrito()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return new JsonResult(new { success = false, message = "Debes iniciar sesión." });
            }

            var id = int.Parse(usuarioId);

            if (ListaId.HasValue)
            {
                var lista = _db.ListasDeseos.FirstOrDefault(l => l.Id == ListaId.Value && l.ClienteId == id);
                if (lista != null && !string.IsNullOrEmpty(lista.Productos))
                {
                    var productosIds = lista.Productos.Split(',').Select(int.Parse).ToList();
                    
                    var carritoJson = HttpContext.Session.GetString("Carrito");
                    var carrito = string.IsNullOrEmpty(carritoJson) 
                        ? new Dictionary<int, int>() 
                        : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(carritoJson) ?? new Dictionary<int, int>();

                    foreach (var prodId in productosIds)
                    {
                        if (carrito.ContainsKey(prodId))
                            carrito[prodId]++;
                        else
                            carrito[prodId] = 1;
                    }

                    HttpContext.Session.SetString("Carrito", System.Text.Json.JsonSerializer.Serialize(carrito));
                }
            }

            return new JsonResult(new { success = true });
        }

        public IActionResult OnPostEliminarLista()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return new JsonResult(new { success = false, message = "Debes iniciar sesión." });
            }

            var id = int.Parse(usuarioId);

            if (ListaId.HasValue)
            {
                var lista = _db.ListasDeseos.FirstOrDefault(l => l.Id == ListaId.Value && l.ClienteId == id);
                if (lista != null)
                {
                    _db.ListasDeseos.Remove(lista);
                    _db.SaveChanges();
                }
            }

            return new JsonResult(new { success = true });
        }

        public IActionResult OnPostEliminarDeLista()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToPage("/Cuentas");
            }

            var id = int.Parse(usuarioId);

            if (ProductoId.HasValue && ListaId.HasValue)
            {
                var lista = _db.ListasDeseos.FirstOrDefault(l => l.Id == ListaId.Value && l.ClienteId == id);
                if (lista != null && !string.IsNullOrEmpty(lista.Productos))
                {
                    var productosActual = lista.Productos.Split(',').Select(int.Parse).ToList();
                    productosActual.Remove(ProductoId.Value);
                    lista.Productos = string.Join(",", productosActual);
                    _db.ListasDeseos.Attach(lista);
                    var entry = _db.Entry(lista);
                    entry.Property(l => l.Productos).IsModified = true;
                    _db.SaveChanges();
                }
            }

            return RedirectToPage("/Cuentas");
        }

        public IActionResult OnPostActualizar()
        {
            Console.WriteLine($"Nombre: {Nombre}");
            Console.WriteLine($"Apellido: {Apellido}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Telefono: {Telefono}");
            Console.WriteLine($"Genero: {Genero}");
            Console.WriteLine($"FechaNacimiento: {FechaNacimiento}");
            
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            var id = int.Parse(usuarioId);
            var cliente = _db.Clientes.Find(id);
            
            if (cliente == null)
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            
            if (!string.IsNullOrEmpty(Nombre)) cliente.Name = Nombre;
            if (!string.IsNullOrEmpty(Apellido)) cliente.Apellido = Apellido;
            if (!string.IsNullOrEmpty(Email)) cliente.Email = Email;
            if (!string.IsNullOrEmpty(Telefono)) cliente.Telefono = Telefono;
            if (FechaNacimiento.HasValue) cliente.fechaNacimiento = FechaNacimiento;
            if (!string.IsNullOrEmpty(Genero)) cliente.Genero = Genero;
            
            
            _db.Clientes.Attach(cliente);
            var entry = _db.Entry(cliente);
            entry.Property(c => c.Name).IsModified = true;
            entry.Property(c => c.Apellido).IsModified = true;
            entry.Property(c => c.Email).IsModified = true;
            entry.Property(c => c.Telefono).IsModified = true;
            entry.Property(c => c.fechaNacimiento).IsModified = true;
            entry.Property(c => c.Genero).IsModified = true;
            _db.SaveChanges();
            
            return RedirectToPage("/Cuentas");
        }
        public IActionResult OnPostAgregarDireccion()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            var id = int.Parse(usuarioId);
            var cliente = _db.Clientes.Find(id);
            
            if (cliente == null)
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            if (!string.IsNullOrEmpty(NuevaDireccion))
            {
                cliente.Direccion = NuevaDireccion;
                _db.Clientes.Attach(cliente);
                var entry = _db.Entry(cliente);
                entry.Property(c => c.Direccion).IsModified = true;
                _db.SaveChanges();
            }
            
            return RedirectToPage("/Cuentas");
        }

        public IActionResult OnPostEliminarDireccion()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            var id = int.Parse(usuarioId);
            var cliente = _db.Clientes.Find(id);
            
            if (cliente == null)
            {
                Response.Redirect("/Login");
                return Page();
            }
            
            if (!string.IsNullOrEmpty(Direccion))
            {
                cliente.Direccion = null;
                _db.Clientes.Attach(cliente);
                var entry = _db.Entry(cliente);
                entry.Property(c => c.Direccion).IsModified = true;
                _db.SaveChanges();
            }
            
            return RedirectToPage("/Cuentas");
        }

        public IActionResult OnPostEliminarProductoLista()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                Response.Redirect("/Login");
                return Page();
            }

            var id = int.Parse(usuarioId);

            if (ProductoId.HasValue && ListaId.HasValue)
            {
                var lista = _db.ListasDeseos.Find(ListaId.Value);
                if (lista != null && lista.ClienteId == id)
                {
                    var productosActual = string.IsNullOrEmpty(lista.Productos)
                        ? new List<int>()
                        : lista.Productos.Split(',').Select(int.Parse).ToList();

                    if (productosActual.Contains(ProductoId.Value))
                    {
                        productosActual.Remove(ProductoId.Value);
                        lista.Productos = string.Join(",", productosActual);
                        _db.ListasDeseos.Attach(lista);
                        var entry = _db.Entry(lista);
                        entry.Property(l => l.Productos).IsModified = true;
                        _db.SaveChanges();
                    }
                }
            }

            return RedirectToPage("/Cuentas");
        }
    }
}