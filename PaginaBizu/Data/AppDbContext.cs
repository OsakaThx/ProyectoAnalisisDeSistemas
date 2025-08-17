using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Models;

namespace PaginaBizu.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión para decimales
            modelBuilder.Entity<Product>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Impuesto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleFactura>()
                .Property(df => df.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleFactura>()
                .Property(df => df.Subtotal)
                .HasPrecision(18, 2);

            // Configuración de relaciones para Factura
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Orden)
                .WithOne()
                .HasForeignKey<Factura>(f => f.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relaciones para DetalleFactura
            modelBuilder.Entity<DetalleFactura>()
                .HasOne(df => df.Factura)
                .WithMany(f => f.Detalles)
                .HasForeignKey(df => df.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleFactura>()
                .HasOne(df => df.Producto)
                .WithMany()
                .HasForeignKey(df => df.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
