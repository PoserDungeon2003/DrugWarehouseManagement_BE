using DrugWarehouseManagement.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class OutboundDetail : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundDetailId { get; set; }
        public int OutboundId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public OutboundDetailStatus Status { get; set; } = OutboundDetailStatus.Pending;
        public decimal UnitPrice { get; set; } 

        public virtual Outbound Outbound { get; set; } = null!; 
    }
}
