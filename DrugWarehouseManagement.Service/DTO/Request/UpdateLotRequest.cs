using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateLotRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LotId must be greater than 0.")]
        public int LotId { get; set; }

        [MaxLength(50, ErrorMessage = "LotNumber cannot exceed 50 characters.")]
        public string? LotNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int? Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TemporaryWarehouse must be a non-negative number.")]
        public int? TemporaryWarehouse { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than 0.")]
        public int? WarehouseId { get; set; } // Permanent Warehouse

        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int? ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int? ProviderId { get; set; }
    }
}
