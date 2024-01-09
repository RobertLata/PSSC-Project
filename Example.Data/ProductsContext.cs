using Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data
{
    public class ProductsContext: DbContext
    {
        public ProductsContext(DbContextOptions<ProductsContext> options) : base(options)
        {
        }

        public DbSet<ProductDto> Product { get; set; }

        public DbSet<ClientDto> Client { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientDto>().ToTable("Client").HasKey(s => s.ClientName);
            modelBuilder.Entity<ProductDto>().ToTable("Product").HasKey(s => s.ProductCode);
        }
    }
}
