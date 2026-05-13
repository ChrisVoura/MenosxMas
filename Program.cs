using MiPrimeraWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorPages();
// ============================================================
// CONEXIÓN A POSTGRESQL 
// ============================================================
string connectionString;

// 1. Intentar leer DATABASE_URL (Railway la inyecta automáticamente)
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Parsear postgresql://user:pass@host:port/db
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var user = WebUtility.UrlDecode(userInfo[0]);
        var pass = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var db = uri.AbsolutePath.TrimStart('/');

        connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=True;Timeout=60";

        Console.WriteLine($"✅ PostgreSQL configurado: Host={host}, Port={port}, DB={db}, User={user}");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"DATABASE_URL tiene formato inválido: {databaseUrl}. Error: {ex.Message}");
    }
}
else
{
    // 2. Fallback: leer de ConnectionStrings:DefaultConnection (para desarrollo local)
    var fallback = builder.Configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrEmpty(fallback))
    {
        // Si el fallback parece SQLite, rechazarlo explícitamente
        if (fallback.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) || 
            fallback.Contains(".db", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "❌ ERROR: La variable DATABASE_URL no está definida en Railway y appsettings.json aún tiene SQLite. " +
                "Solución: Ve al dashboard de Railway → tu servicio .NET → Variables, y asegúrate de que DATABASE_URL exista. " +
                "Si no existe, agrégala manualmente copiando la URL de Private Network desde el servicio PostgreSQL."
            );
        }

        connectionString = fallback;
        Console.WriteLine("⚠️  Usando DefaultConnection de appsettings.json (modo desarrollo)");
    }
    else
    {
        throw new InvalidOperationException(
            "❌ ERROR: No se encontró DATABASE_URL (Railway) ni DefaultConnection (appsettings.json). " +
            "La app no puede conectarse a la base de datos."
        );
    }
}

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.FormFieldName = "__RequestVerificationToken";
});

builder.Services.AddResponseCaching();

var app = builder.Build();

// Aplicar migraciones al iniciar
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.Migrate();
// }

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/admin") || context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";
    }
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAntiforgery();
app.UseAuthorization();
app.UseForwardedHeaders();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// === API DE CARRITO ===
app.MapGet("/api/carrito/count", (HttpContext ctx) => {
    var json = ctx.Session.GetString("Carrito");
    if (string.IsNullOrEmpty(json)) return Results.Ok("0");
    try {
        var cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int,int>>(json);
        return Results.Ok(cart.Values.Sum().ToString());
    } catch { return Results.Ok("0"); }
});

app.MapMethods("/api/carrito/agregar", new[] { "GET", "POST" }, async (HttpContext ctx) => {
    var id = 0;
    if (ctx.Request.Method == "POST") {
        var form = await ctx.Request.ReadFormAsync();
        int.TryParse(form["id"], out id);
    } else {
        int.TryParse(ctx.Request.Query["id"], out id);
    }
    if (id <= 0) return Results.BadRequest();
    var json = ctx.Session.GetString("Carrito");
    var cart = new Dictionary<int,int>();
    if (!string.IsNullOrEmpty(json)) {
        try { cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int,int>>(json); } catch {}
    }
    if (cart.ContainsKey(id)) cart[id]++; else cart[id] = 1;
    ctx.Session.SetString("Carrito", System.Text.Json.JsonSerializer.Serialize(cart));
    return Results.Ok("ok");
});

app.MapGet("/api/carrito/html", (HttpContext ctx) => {
    var json = ctx.Session.GetString("Carrito");
    if (string.IsNullOrEmpty(json)) return Results.Ok("<p class='text-center p-3'>Tu carrito está vacío</p>");
    try {
        var db = ctx.RequestServices.GetService<MiPrimeraWebApp.Data.AppDbContext>();
        var cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int,int>>(json);
        if (cart.Count == 0) return Results.Ok("<p class='text-center p-3'>Tu carrito está vacío</p>");
        var ps = db.Productos.Where(p => cart.Keys.Contains(p.Id)).ToList();
        var total = cart.Sum(x => ps.First(p => p.Id == x.Key).Price * x.Value);

        var html = "<div class='p-3'>";
        foreach (var p in ps)
        {
            var cant = cart[p.Id];
            var img = p.ImageUrl?.Split(',').FirstOrDefault();
            string card = "<div class='card mb-2'>" +
            "<div class='card-body d-flex justify-content-between align-items-center'>" +
                "<div class='d-flex align-items-center'>" +
                    "<img src='" + img + "' class='img-thumbnail me-3' style='width:60px;height:60px;object-fit:cover;'/>" +
                    "<div>" +
                        "<h6 class='card-title mb-1'>" + p.Name + "</h6>" +
                        "<p class='card-text mb-0'>₡" + p.Price.ToString("F3") + "</p>" +
                        "<div class='d-flex align-items-center mt-1'>" +
                            "<button class='btn btn-sm btn-outline-secondary' onclick='cambiarCantidad(" + p.Id + ",-1)'>-</button>" +
                            "<span class='mx-2'>" + cant + "</span>" +
                            "<button class='btn btn-sm btn-outline-secondary' onclick='cambiarCantidad(" + p.Id + ",1)'>+</button>" +
                        "</div>" +
                    "</div>" +
                "</div>" +
                "<button class='btn p-0 border-0 bg-transparent' onclick='eliminarProducto(" + p.Id + ")'> <img width='30' height='30' src='https://img.icons8.com/stickers/100/delete-forever.png' alt='delete-forever'/></button>" +
            "</div>" +
        "</div>";
        html += card;
        }
        html += "</div>";
        html += "<div class='p-3'><div class='d-flex justify-content-between mb-2'><span class='fw-bold'>Total:</span><span class='fw-bold fs-5'>₡" + total.ToString("F3") + "</span></div><button class='btn btn-outline-danger w-100 mb-2' onclick='limpiarCarrito()'>Limpiar Carrito</button><button class='btn btn-success w-100'>Finalizar Compra</button></div>";

        return Results.Ok(html);
    } catch { return Results.Ok("<p class='text-center p-3'>Tu carrito está vacío</p>"); }
});

app.MapMethods("/api/carrito/limpiar", new[] { "GET", "POST" }, (HttpContext ctx) => {
    ctx.Session.Remove("Carrito");
    return Results.Ok("ok");
});

app.MapMethods("/api/carrito/cambiar", new[] { "GET", "POST" }, (HttpContext ctx) => {
    var query = ctx.Request.Query;
    if (!int.TryParse(query["id"], out int id) || !int.TryParse(query["delta"], out int delta)) return Results.BadRequest();
    var json = ctx.Session.GetString("Carrito");
    var cart = new Dictionary<int,int>();
    if (!string.IsNullOrEmpty(json)) {
        try { cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int,int>>(json); } catch {}
    }
    if (cart.ContainsKey(id)) {
        cart[id] += delta;
        if (cart[id] <= 0) cart.Remove(id);
    }
    ctx.Session.SetString("Carrito", System.Text.Json.JsonSerializer.Serialize(cart));
    return Results.Ok("ok");
});

app.MapMethods("/api/carrito/eliminar", new[] { "GET", "POST" }, (HttpContext ctx) => {
    if (!int.TryParse(ctx.Request.Query["id"], out int id)) return Results.BadRequest();
    var json = ctx.Session.GetString("Carrito");
    if (!string.IsNullOrEmpty(json)) {
        try {
            var cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int,int>>(json);
            cart.Remove(id);
            ctx.Session.SetString("Carrito", System.Text.Json.JsonSerializer.Serialize(cart));
        } catch {}
    }
    return Results.Ok("ok");
});

app.Run();