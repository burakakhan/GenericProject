using GenericProject.Contracts.Enums;
using GenericProject.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericProject.Domain.Entities
{
    public class Product:BaseEntity
    {
        [Required] // Domain seviyesinde de kural olabilir
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        //public int Status { get; set; } = 1; // 1: Active, 0: Inactive

        public ProductStatus Status { get; set; } = ProductStatus.Active; // Enum kullanımı
    }
}
