using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class OutboundDetails : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundDetailsId { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public float Discount { get; set; }
      
        public int OutboundId { get; set; }
        public int LotId { get; set; }

        public virtual Lot Lot { get; set; } = null!;
        public virtual Outbound Outbound { get; set; } = null!;
        public virtual ICollection<ReturnOutboundDetails> ReturnOutboundDetails { get; set; }
       = new List<ReturnOutboundDetails>();

    }
}
