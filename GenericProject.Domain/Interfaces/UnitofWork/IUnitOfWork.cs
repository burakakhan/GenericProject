using GenericProject.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Domain.Interfaces.UnitofWork
{
    public interface IUnitOfWork: IDisposable
    {
        IProductRepository ProductRepository { get; }

        // Diğer Repository Interface'leri buraya eklenebilir BURAKEKLE
        Task<int> CompleteAsync();
    }
}
