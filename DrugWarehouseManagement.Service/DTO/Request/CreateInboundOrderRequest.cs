using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateInboundOrderRequest
    {
        [MaxLength(1000, ErrorMessage = "Note must be at most 1000 characters.")]
        public string? Note { get; set; }
        public List<InboundOrderDetailRequest>? InboundRequestDetails { get; set; }
    }

    public class InboundOrderDetailRequest
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number.")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "UnitPrice is required.")]
        [Range(0.1, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.1")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "TotalPrice is required.")]
        [Range(0.1, double.MaxValue, ErrorMessage = "TotalPrice must be greater than 0.1")]
        public decimal TotalPrice { get; set; }
    }
}
