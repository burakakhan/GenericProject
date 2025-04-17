using GenericProject.Domain.Interfaces.Repositories;
using GenericProject.Domain.Interfaces.UnitofWork;
using GenericProject.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IProductRepository? _productRepository;
        // Diğer repository'ler için private field'lar BURAKEKLE

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }


        // Repository'leri lazy loading ile oluşturma
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);

        // Diğer repository'ler için public property'ler BURAKEKLE

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
