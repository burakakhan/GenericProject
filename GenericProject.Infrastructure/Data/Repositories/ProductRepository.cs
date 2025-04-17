using GenericProject.Contracts.Enums;
using GenericProject.Domain.Entities;
using GenericProject.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Infrastructure.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Status> status)                
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // Gerekirse diğer GenericRepository metotlarını override edebilirsiniz.
        // Örneğin: GetByIdAsync'i override edip ilişkili verileri de çekmek (Include)
        public override async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // Örnek olarak Include eklenmedi ama burada yapılabilir.
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
    }
}
