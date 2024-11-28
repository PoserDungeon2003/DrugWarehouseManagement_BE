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
    public class Outbound : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundId { get; set; }
     
        public string OutboundCode { get; set; }
        
        public Guid AccountId { get; set; } 
        public DateTime? OutboundDate { get; set; } = DateTime.Now;
        public int CustomerId { get; set; } 
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }

        public decimal VAT { get; set; }
        public decimal ShippingCost { get; set; }
        public OutboundStatus Status { get; set; } = OutboundStatus.Pending;

        public virtual List<OutboundDetail> OutboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual List<Product> Product { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!; 



    }
}
