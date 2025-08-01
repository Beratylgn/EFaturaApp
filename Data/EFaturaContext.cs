using EFaturaApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EFaturaApp.Data
{
    public class EFaturaContext : DbContext
    {
        public EFaturaContext(DbContextOptions<EFaturaContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
