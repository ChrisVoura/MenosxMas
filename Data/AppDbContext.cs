using Microsoft.EntityFrameworkCore;

namespace MiPrimeraWebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<ListaDeseo> ListasDeseos { get; set; } = null!;
    public DbSet<Empleado> Empleados { get; set; } = null!;
    public DbSet<Comentario> Comentarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>().ToTable("productos");
        modelBuilder.Entity<Cliente>().ToTable("clientes");
        modelBuilder.Entity<Pedido>().ToTable("pedidos");
        modelBuilder.Entity<ListaDeseo>().ToTable("listas_deseos");
        modelBuilder.Entity<Empleado>().ToTable("empleados");
        modelBuilder.Entity<Comentario>().ToTable("comentarios");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }
        }
    }
}