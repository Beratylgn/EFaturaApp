using Microsoft.EntityFrameworkCore;
using EFaturaApp.Models;

namespace EFaturaApp.Data
{
    public class EFaturaContext : DbContext
    {
        public EFaturaContext(DbContextOptions<EFaturaContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("CUSTOMER");
            modelBuilder.Entity<Product>().ToTable("PRODUCT");
            modelBuilder.Entity<Shipment>().ToTable("SHIPMENTS");
            modelBuilder.Entity<ShipmentItem>().ToTable("SHIPMENTITEMS");
            modelBuilder.Entity<Invoice>().ToTable("INVOICES");
            modelBuilder.Entity<InvoiceItem>().ToTable("INVOICESITEMS");
            modelBuilder.Entity<User>().ToTable("USERS");

            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Items)
                .WithOne(ii => ii.Invoice!)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
