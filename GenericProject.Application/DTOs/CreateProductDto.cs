using GenericProject.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace GenericProject.Application.DTOs
{
    public class CreateProductDto
    {
        [Required] // Domain seviyesinde de kural olabilir
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        public ProductStatus Status { get; set; }
    }
}
