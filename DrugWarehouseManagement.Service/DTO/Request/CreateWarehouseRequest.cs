using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateWarehouseRequest
    {
        [Required]
        public string WarehouseName { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;
        [Required]
        public string DocumentNumber { get; set; } = null!;
        [Required]
        public string WarehouseCode { get; set; } = null!;

    }
}
