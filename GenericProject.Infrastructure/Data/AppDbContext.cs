using GenericProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        // Diğer DbSet'ler burada tanımlanabilir BURAKEKLE

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Model konfigürasyonları burada yapılabilir (Fluent API)
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

            // Örnek veri ekleme (InMemory DB için)
            modelBuilder.Entity<Product>().HasData(
               new Product { Id = 1, Name = "Laptop", Price = 1500.00m, Status = Contracts.Enums.ProductStatus.Active, CreatedDate = new DateTime(2023, 01, 01) },
               //new Product { Id = 2, Name = "Keyboard", Price = 75.50m, Status = Contracts.Enums.ProductStatus.Inactive, CreatedDate = DateTime.UtcNow },
               new Product { Id = 3, Name = "Mouse", Price = 25.00m, Status = Contracts.Enums.ProductStatus.Discontinued, CreatedDate = new DateTime(2023, 01, 01) }
           );
        }
    }
}
