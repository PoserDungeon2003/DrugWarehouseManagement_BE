using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateProductRequest
    {
        public string? ProductName { get; set; } = null!;
        public string? ProductCode { get; set; } = null!;

       
        public string? Type { get; set; } = null!;

     
        public string? MadeFrom { get; set; } = null!;

       
        public int? ProviderId { get; set; }
        public int? CategoryId { get; set; }
    }
}
