using EFaturaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EFaturaApp.Data
{
    public class EFaturaContext : DbContext
    {
        public EFaturaContext(DbContextOptions<EFaturaContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<Invoice> INVOICES { get; set; }
        public DbSet<InvoiceItem> INVOICESITEMS { get; set; }
        public DbSet<Customer> CUSTOMERS { get; set; }
        public DbSet<Product> PRODUCT { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.ToTable("SHIPMENTS");

                entity.Property(s => s.Id).HasColumnName("ID");
                entity.Property(s => s.ShipmentNo).HasColumnName("SHIPMENTNO");
                entity.Property(s => s.SenderUserId).HasColumnName("SENDERUSERID");
                entity.Property(s => s.RecieverCustomerId).HasColumnName("RECIEVERCUSTOMERID");
                entity.Property(s => s.ShipmentDate).HasColumnName("SHIPMENTDATE");
                entity.Property(s => s.CreateDate).HasColumnName("CREATEDATE");

                entity.HasOne(s => s.RecieverCustomer)
                      .WithMany()
                      .HasForeignKey(s => s.RecieverCustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SenderUser)
                      .WithMany()
                      .HasForeignKey(s => s.SenderUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ShipmentItem>(entity =>
            {
                entity.ToTable("SHIPMENTITEMS");

                entity.Property(si => si.Id).HasColumnName("ID");
                entity.Property(si => si.ShipmentId).HasColumnName("SHIPMENTID");
                entity.Property(si => si.ProductId).HasColumnName("PRODUCTID");
                entity.Property(si => si.Quantity).HasColumnName("QUANTITY");
                entity.Property(si => si.Unit).HasColumnName("UNIT");

                entity.HasOne(si => si.Shipment)
                      .WithMany(s => s.ShipmentItems)
                      .HasForeignKey(si => si.ShipmentId);

                entity.HasOne(si => si.Product)
                      .WithMany()
                      .HasForeignKey(si => si.ProductId);
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.ToTable("INVOICESITEMS");

                entity.Property(i => i.ID).HasColumnName("ID");
                entity.Property(i => i.INVOICEID).HasColumnName("INVOICEID");
                entity.Property(i => i.PRODUCTID).HasColumnName("PRODUCTID");
                entity.Property(i => i.QUANTITY).HasColumnName("QUANTITY");
                entity.Property(i => i.UNITPRICE).HasColumnName("UNITPRICE");
                entity.Property(i => i.TAXRATE).HasColumnName("TAXRATE");

                entity.HasOne(i => i.Invoice)
                      .WithMany(i => i.InvoiceItems)
                      .HasForeignKey(i => i.INVOICEID);

                entity.HasOne(i => i.Product)
                      .WithMany()
                      .HasForeignKey(i => i.PRODUCTID);
            });
        }
    }
}

