using GenericProject.Application.DTOs;
using GenericProject.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Application.Interfaces.Services
{
    public interface IProductService : IGenericService<ProductDto, CreateProductDto, UpdateProductDto>
    {
        

        // Product'a özel servis metotları
        Task<IEnumerable<ProductDto>> GetProductsByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default);
        Task UpdateProductStockAsync(int id, int newStockQuantity, CancellationToken cancellationToken = default);
    }
}
