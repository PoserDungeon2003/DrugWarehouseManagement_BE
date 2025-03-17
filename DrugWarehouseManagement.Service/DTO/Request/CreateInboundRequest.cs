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
    public class CreateInboundRequest
    {
        public string? ProviderOrderCode { get; set; }
        [MaxLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Note { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int ProviderId { get; set; }
        public int? WarehouseId { get; set; } // Warehouse
    }

}
