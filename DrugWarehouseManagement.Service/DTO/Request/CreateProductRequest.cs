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
        public int[]? CategoriesIds { get; set; }
    }
}
