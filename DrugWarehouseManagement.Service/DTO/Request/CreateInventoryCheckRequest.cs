using DrugWarehouseManagement.Common;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateInventoryCheckRequest
    {
        [MaxLength(1000, ErrorMessage = "Title cannot exceed 1000 characters.")]
        public string Title { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than 0.")]
        public int WarehouseId { get; set; }
        [MaxLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Notes { get; set; } // Ghi chú tổng quan
        public List<InventoryCheckDetailRequest>? Details { get; set; }
    }

    public class InventoryCheckDetailRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LotId must be greater than 0.")]
        public int LotId { get; set; }

        public InventoryCheckStatus Status { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
        [MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        public string Reason { get; set; } = null!;
        [MaxLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Notes { get; set; } // Ghi chú chi tiết
    }
}
