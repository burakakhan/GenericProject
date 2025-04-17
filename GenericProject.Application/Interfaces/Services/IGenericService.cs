using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Application.Interfaces.Services
{
    public interface IGenericService<TGetDto, TCreateDto, TUpdateDto>
    {
        Task<TGetDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TGetDto> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
