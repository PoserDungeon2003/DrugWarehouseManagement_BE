using DrugWarehouseManagement.Common.Enums;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class OutboundDetails : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundDetailsId { get; set; }
        public string LotNumber { get; set; } = null!;
        public Instant ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string UnitType { get; set; } = null!;
        public OutboundDetailStatus Status { get; set; } = OutboundDetailStatus.Pending;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int ProductId { get; set; }
        public int OutboundId { get; set; }
        public int LotId { get; set; }  
		public virtual Lot Lot { get; set; } = null!;   
		public virtual Outbound Outbound { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
