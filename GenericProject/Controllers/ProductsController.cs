using GenericProject.Application.DTOs;
using GenericProject.Application.Interfaces.Services;
using GenericProject.Contracts.Enums;
using GenericProject.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using GenericProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations; // API Versiyonlama

namespace GenericProject.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] // Bu controller'ın versiyonu
    [Route("api/v{version:apiVersion}/[controller]")] // Versiyonlu route
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        //private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService/*, ILogger<ProductsController> logger*/)
        {
            _productService = productService;
            //_logger = logger;
        }

        // GET: api/v1/products
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Getting all products");
            var products = await _productService.GetAllAsync(cancellationToken);
            var response = ApiResponse<IEnumerable<ProductDto>>.Success(products);
            return Ok(response);
        }

        // GET: api/v1/products/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Getting product with ID: {ProductId}", id);
            var product = await _productService.GetByIdAsync(id, cancellationToken);

            if (id == 15)
            {
                return NotFound(ApiResponse<ProductDto>.Fail($"Product2 with ID {id} not found.", StatusCodes.Status500InternalServerError));
            }

            if (id == 16)
            {
                throw new ValidationException($"Validation request");
            }



            if (product == null)
            {
                //_logger.LogWarning("Product with ID: {ProductId} not found", id);
                // Middleware NotFoundException'ı yakalayacağı için burada doğrudan 404 dönmek yerine
                // null check yapıp uygun response dönmek daha iyi olabilir.
                // Ya da Service katmanında NotFoundException fırlatılır.
                // Bu örnekte Service null dönüyor, Controller 404 üretiyor.
                throw new NotFoundException($"Ürün bulunamadı: ID = {id}");
                //return NotFound(ApiResponse<ProductDto>.Fail($"Product2 with ID {id} not found.", StatusCodes.Status404NotFound));
            }

            var response = ApiResponse<ProductDto>.Success(product);
            return Ok(response);
        }

        // POST: api/v1/products
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)] // Validation hataları için
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createDto, CancellationToken cancellationToken)
        {
            // Model binding validation'ı [ApiController] attribute'u otomatik yapar.
            // FluentValidation hataları middleware tarafından yakalanır.
            if (!ModelState.IsValid) // Attribute bazlı validation (ekstra kontrol)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ProductDto>.Fail(errors, StatusCodes.Status400BadRequest));
            }

            //_logger.LogInformation("Creating a new product with name: {ProductName}", createDto.Name);
            var createdProduct = await _productService.CreateAsync(createDto, cancellationToken);
            var response = ApiResponse<ProductDto>.Success(createdProduct, StatusCodes.Status201Created);

            // Oluşturulan kaynağın URI'sini Location header'ına ekle
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id, version = "1.0" }, response);
        }

        // PUT: api/v1/products/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)] // Başarılı güncellemede içerik dönmüyoruz
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)] // Validation hataları
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)] // Kaynak bulunamazsa
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail(errors, StatusCodes.Status400BadRequest));
            }

            //_logger.LogInformation("Updating product with ID: {ProductId}", id);
            // Service katmanı NotFoundException fırlatacak (eğer bulunamazsa), middleware yakalayacak.
            // Service katmanı ValidationException fırlatacak (eğer valid değilse), middleware yakalayacak.
            await _productService.UpdateAsync(id, updateDto, cancellationToken);

            // Başarılı güncelleme için 204 No Content dönüyoruz. ApiResponse sarmalayıcısı ile.
            return Ok(ApiResponse<object>.Success(StatusCodes.Status204NoContent)); // NoContent() yerine custom response
            // VEYA
            // return NoContent(); // Direkt 204 döner, ApiResponse olmadan. Tercihe bağlı.
        }

        // DELETE: api/v1/products/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)] // Başarılı silmede içerik dönmüyoruz
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)] // Kaynak bulunamazsa
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Deleting product with ID: {ProductId}", id);
            // Service katmanı NotFoundException fırlatacak (eğer bulunamazsa), middleware yakalayacak.
            await _productService.DeleteAsync(id, cancellationToken);

            // Başarılı silme için 204 No Content dönüyoruz. ApiResponse sarmalayıcısı ile.
            return Ok(ApiResponse<object>.Success(StatusCodes.Status204NoContent)); // NoContent() yerine custom response
                                                                                    // VEYA
                                                                                    // return NoContent(); // Direkt 204 döner, ApiResponse olmadan.
        }

        // Örnek özel endpoint: Duruma göre ürünleri getir
        // GET: api/v1/products/status/Active
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)] // Geçersiz enum değeri için
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByStatus(ProductStatus status, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Getting products with status: {ProductStatus}", status);
            // Enum binding otomatik olarak geçersiz değerleri yakalayabilir (400 Bad Request döner)
            var products = await _productService.GetProductsByStatusAsync(status, cancellationToken);
            var response = ApiResponse<IEnumerable<ProductDto>>.Success(products);
            return Ok(response);
        }
    }
}
