using AutoMapper;
using FluentValidation;
using GenericProject.Application.DTOs;
using GenericProject.Application.Interfaces.Services;
using GenericProject.Contracts.Enums;
using GenericProject.Domain.Entities;
using GenericProject.Domain.Exceptions;
using GenericProject.Domain.Interfaces.UnitofWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Application.Services
{
    public class ProductService:IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateProductDto> _createValidator; // FluentValidation validator'ları
        private readonly IValidator<UpdateProductDto> _updateValidator;
        //private readonly IMemoryCache _cache; // Cache
        //private readonly ILogger<ProductService> _logger; // Logger

        private const string ProductCachePrefix = "Product_"; // Cache anahtarı ön eki
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // Cache süresi

        // Loglama ve Cache burada olabilir BURAKEKLE
        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id, cancellationToken);
            var productDto = _mapper.Map<ProductDto>(product);
            return productDto;
        }

        public async  Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createDto, CancellationToken cancellationToken = default)
        {
            // 1. Validation
            var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                // FluentValidation kendi exception'ını fırlatır veya biz custom exception fırlatabiliriz.
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Mapping
            var product = _mapper.Map<Product>(createDto);

            // 3. Add to Repository & Commit
            await _unitOfWork.ProductRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.CompleteAsync(); // Değişiklikleri kaydet

            // 4. Return DTO
            return _mapper.Map<ProductDto>(product); // Oluşturulan entity'nin ID'sini içeren DTO'yu dön
        }

        public async Task UpdateAsync(int id, UpdateProductDto updateDto, CancellationToken cancellationToken = default)
        {
            // 1. Validation
            var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                // FluentValidation kendi exception'ını fırlatır veya biz custom exception fırlatabiliriz.
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Get Existing Entity
            var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(id, cancellationToken);
            if (existingProduct == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }

            // 3. Mapping (Update existing entity)
            _mapper.Map(updateDto, existingProduct); // Var olan entity'i güncelle


            // 4. Update in Repository & Commit
            _unitOfWork.ProductRepository.Update(existingProduct); // EF Core değişikliği takip eder
            await _unitOfWork.CompleteAsync();

        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            // 1. Get Existing Entity
            var productToDelete = await _unitOfWork.ProductRepository.GetByIdAsync(id, cancellationToken);
            if (productToDelete == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }

            // 2. Remove from Repository & Commit
            _unitOfWork.ProductRepository.Remove(productToDelete);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.ProductRepository.GetProductsByStatusAsync(status, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task UpdateProductStockAsync(int id, int newStockQuantity, CancellationToken cancellationToken = default)
        {
            if (newStockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(newStockQuantity));
            }

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }


            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.CompleteAsync();
        }

      
    }
}
