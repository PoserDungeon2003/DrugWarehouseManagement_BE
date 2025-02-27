using DrugWarehouseManagement.Common.Enums;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InboundDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundDetailsId { get; set; }
        public string LotNumber { get; set; } = null!;
        public Instant? ManufacturingDate { get; set; }
        public Instant? ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int InboundId { get; set; }
        public InboundDetailStatus Status { get; set; } = InboundDetailStatus.Pending;

        public virtual Inbound Inbound { get; set; } = null!;

    }
}
