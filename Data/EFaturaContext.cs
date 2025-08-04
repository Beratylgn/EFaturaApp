using Microsoft.EntityFrameworkCore;
using EFaturaApp.Models;

namespace EFaturaApp.Data
{
    public class EFaturaContext : DbContext
    {
        public EFaturaContext(DbContextOptions<EFaturaContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }

    }
}
