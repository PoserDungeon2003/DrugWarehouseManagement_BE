using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InboundRequestDetails
    {
        [Key]
        public int InboundRequestDetailsId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int ProductId { get; set; }
        public int InboundRequestId { get; set; }
    
        public virtual Product Product { get; set; } = null!;
        public virtual InboundRequest InboundRequest { get; set; } = null!;
    }
}
