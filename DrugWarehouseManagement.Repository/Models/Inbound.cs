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
    public class Inbound : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundId { get; set; }
        public string InboundCode { get; set; } = null!;   
        public Guid AccountId { get; set; }
        public DateTime? InboundDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDeliveryDay { get; set; }
        public InboundStatus Status { get; set; }  = InboundStatus.Pending;
        public int SupplierId { get; set; }

        public virtual List<InboundDetail> InboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual List<Drug> Product { get; set; } = null!;

    }
}
