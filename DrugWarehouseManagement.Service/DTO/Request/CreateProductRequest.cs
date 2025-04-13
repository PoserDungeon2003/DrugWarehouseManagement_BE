using DrugWarehouseManagement.Repository.Models;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateProductRequest
    {
        [Required]
        public string ProductName { get; set; } = null!;
        [Required]
        public string ProductCode { get; set; } = null!;
        [Required]
        public string SKU { get; set; } = null!;
        [Required]
        public string MadeFrom { get; set; } = null!;
        public List<ProductCategoriesRequest>? ProductCategories { get; set; }
    }

    public class ProductCategoriesRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "CategoriesId must be a positive number.")]
        public int CategoriesId { get; set; }
    }
}
