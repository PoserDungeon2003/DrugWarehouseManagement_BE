using DrugWarehouseManagement.Repository.Models;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateProductRequest
    {
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? SKU { get; set; }
        public string? MadeFrom { get; set; }
        
        public List<ProductCategoriesRequest>? ProductCategories { get; set; }
    }
}
