using GenericProject.Contracts.Enums;
using GenericProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Domain.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Örnek: Product'a özel bir sorgu metodu
        Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default);
    }
}
